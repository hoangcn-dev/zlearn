using System.Windows;
using System.Windows.Controls;

namespace ZLearn.AdminDesktopApp.Views
{
    public partial class SuccessTextBlock : UserControl
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(SuccessTextBlock),
                new PropertyMetadata(string.Empty));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public SuccessTextBlock()
        {
            InitializeComponent();
        }
    }
}
