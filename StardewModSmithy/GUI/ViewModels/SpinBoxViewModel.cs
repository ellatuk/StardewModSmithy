using System.ComponentModel;
using StardewModSmithy.Integration;

namespace StardewModSmithy.GUI.ViewModels;

public partial class SpinBoxViewModel(Func<int> backingGetter, Action<int> backingSetter, int minimum, int maximum)
    : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Value
    {
        get => backingGetter();
        set
        {
            if (value < minimum || value > maximum)
                return;
            int prevValue = backingGetter();
            backingSetter(value);
            if (prevValue != backingGetter())
                PropertyChanged?.Invoke(this, new(nameof(Value)));
        }
    }

    public void Decrease() => Value -= 1;

    public void Increase() => Value += 1;

    public void Wheel(SDUIDirection direction)
    {
        switch (direction)
        {
            case SDUIDirection.North:
                Increase();
                break;
            case SDUIDirection.South:
                Decrease();
                break;
        }
    }
}
