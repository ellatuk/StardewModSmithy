using System.ComponentModel;
using StardewModSmithy.Integration;

namespace StardewModSmithy.GUI.ViewModels;

public class AbstractSpinBoxViewModel<T>(Func<T> backingGetter, Action<T> backingSetter) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public void InvokePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(sender, e);
    }

    public T Value
    {
        get => ValueGetter();
        set => ValueSetter(value);
    }
    public string ValueLabel => ValueLabelGetter();

    public virtual T ValueGetter() => backingGetter();

    public virtual void ValueSetter(T newValue)
    {
        backingSetter(newValue);
        PropertyChanged?.Invoke(this, new(nameof(Value)));
        PropertyChanged?.Invoke(this, new(nameof(ValueLabel)));
    }

    public virtual string ValueLabelGetter() => Value?.ToString() ?? string.Empty;

    public virtual void Decrease() { }

    public virtual void Increase() { }

    public void Wheel(SDUIDirection direction)
    {
        switch (direction)
        {
            case SDUIDirection.North:
                Decrease();
                break;
            case SDUIDirection.South:
                Increase();
                break;
        }
    }
}

public class IntSpinBoxViewModel(Func<int> backingGetter, Action<int> backingSetter, int minimum, int maximum)
    : AbstractSpinBoxViewModel<int>(backingGetter, backingSetter)
{
    public override void ValueSetter(int newValue)
    {
        if (newValue < minimum || newValue > maximum)
            return;
        base.ValueSetter(newValue);
    }

    public override void Decrease() => Value -= 1;

    public override void Increase() => Value += 1;
}

public class IBoundsProviderSpinBoxViewModel(
    Func<IBoundsProvider?> backingGetter,
    Action<IBoundsProvider?> backingSetter
) : AbstractSpinBoxViewModel<IBoundsProvider?>(backingGetter, backingSetter)
{
    public record BoundsProviderSelectEntry(IBoundsProvider BoundsProvider, bool IsSelected)
    {
        public string UILabel => BoundsProvider.UILabel;
    }

    internal IReadOnlyList<IBoundsProvider> BoundsProviderList = [];
    private int currentIdx = -1;
    private int MaxIdx => BoundsProviderList.Count - 1;

    private void SetValueToCurrentIndex()
    {
        if (currentIdx >= 0 && currentIdx <= MaxIdx)
            Value = BoundsProviderList[currentIdx];
        else
            Value = null;
        ViewingBoundsProviderList = false;
        InvokePropertyChanged(this, new(nameof(FilteredBoundsProviderList)));
    }

    public void SeekIndex()
    {
        if (Value == null)
        {
            currentIdx = -1;
        }
        else
        {
            currentIdx = -1;
            foreach (IBoundsProvider? prov in BoundsProviderList)
            {
                currentIdx++;
                if (prov == Value)
                    break;
            }
        }
        SetValueToCurrentIndex();
    }

    public void ClampIndex()
    {
        if (MaxIdx == -1)
        {
            currentIdx = -1;
        }
        else if (currentIdx < 0)
        {
            currentIdx = 0;
        }
        else if (currentIdx > MaxIdx)
        {
            currentIdx = MaxIdx;
        }
        SetValueToCurrentIndex();
    }

    public override void Decrease()
    {
        currentIdx = currentIdx <= 0 ? MaxIdx : currentIdx - 1;
        SetValueToCurrentIndex();
    }

    public override void Increase()
    {
        currentIdx = currentIdx >= MaxIdx ? 0 : currentIdx + 1;
        SetValueToCurrentIndex();
    }

    public override string ValueLabelGetter() => Value?.UILabel ?? "NULL";

    public IEnumerable<BoundsProviderSelectEntry> FilteredBoundsProviderList =>
        (
            string.IsNullOrEmpty(BoundsProviderSearchTerm)
                ? BoundsProviderList
                : BoundsProviderList.Where(bp => bp.UILabel.Contains(BoundsProviderSearchTerm))
        ).Select(bp => new BoundsProviderSelectEntry(bp, bp == Value));

    private bool viewingBoundsProviderList = false;
    public bool ViewingBoundsProviderList
    {
        get => viewingBoundsProviderList;
        set
        {
            viewingBoundsProviderList = value;
            InvokePropertyChanged(this, new(nameof(ViewingBoundsProviderList)));
        }
    }

    public void ToggleViewingBoundsProviderList()
    {
        ViewingBoundsProviderList = !ViewingBoundsProviderList;
    }

    public void SelectBoundsProvider(BoundsProviderSelectEntry entry)
    {
        Value = entry.BoundsProvider;
        SeekIndex();
    }

    private string boundsProviderSearchTerm = string.Empty;
    public string BoundsProviderSearchTerm
    {
        get => boundsProviderSearchTerm;
        set
        {
            boundsProviderSearchTerm = value;
            InvokePropertyChanged(this, new(nameof(BoundsProviderSearchTerm)));
            InvokePropertyChanged(this, new(nameof(FilteredBoundsProviderList)));
        }
    }
}
