using System.ComponentModel;
using StardewModdingAPI;
using StardewModSmithy.Integration;

namespace StardewModSmithy.GUI.ViewModels;

public partial class AbstractSpinBoxViewModel<T>(Func<T> backingGetter, Action<T> backingSetter)
    : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public T Value
    {
        get => ValueGetter();
        set => ValueSetter(value);
    }
    public string ValueLabel => ValueLabelGetter();

    public virtual T ValueGetter() => backingGetter();

    public virtual void ValueSetter(T newValue)
    {
        T prevValue = backingGetter();
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

public partial class IntSpinBoxViewModel(Func<int> backingGetter, Action<int> backingSetter, int minimum, int maximum)
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

public partial class IBoundsProviderSpinBoxViewModel(
    Func<IBoundsProvider?> backingGetter,
    Action<IBoundsProvider?> backingSetter
) : AbstractSpinBoxViewModel<IBoundsProvider?>(backingGetter, backingSetter)
{
    internal IReadOnlyList<IBoundsProvider?> furnitureDataList = [];
    private int currentIdx = 0;
    private int MaxIdx => furnitureDataList.Count - 1;

    public override void Decrease()
    {
        currentIdx = currentIdx <= 0 ? MaxIdx : currentIdx - 1;
        Value = furnitureDataList[currentIdx];
    }

    public override void Increase()
    {
        currentIdx = currentIdx >= MaxIdx ? 0 : currentIdx + 1;
        Value = furnitureDataList[currentIdx];
    }

    public override string ValueLabelGetter() => Value?.UILabel ?? string.Empty;
}
