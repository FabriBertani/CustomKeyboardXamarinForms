using System.ComponentModel;
using Xamarin.Forms;

namespace CustomKeyboard
{
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // Here we implement the action of the Enter button on our custom keyboard
            this.entry1.EnterCommand = new Command(() => this.entry2.Focus());
            this.entry2.EnterCommand = new Command(() => this.entry1.Focus());
        }
    }
}