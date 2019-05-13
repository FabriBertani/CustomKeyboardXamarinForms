using System.Windows.Input;
using Xamarin.Forms;

namespace CustomKeyboard
{
    public class EntryWithCustomKeyboard : Entry
    {
        public static readonly BindableProperty EnterCommandProperty = BindableProperty.Create(
            nameof(EnterCommand),
            typeof(ICommand),
            typeof(EntryWithCustomKeyboard),
            default(ICommand),
            BindingMode.OneWay
        );

        public ICommand EnterCommand
        {
            get => (ICommand)GetValue(EnterCommandProperty);
            set => SetValue(EnterCommandProperty, value);
        }
    }
}
