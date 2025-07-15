using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZLearn.AdminDesktopApp.Views
{
    /// <summary>
    /// Interaction logic for ErrorTextBlock.xaml
    /// </summary>
    public partial class ErrorTextBlock : UserControl
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(ErrorTextBlock),
                new PropertyMetadata(string.Empty));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public ErrorTextBlock()
        {
            InitializeComponent();
        }
    }
}
