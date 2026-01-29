using System.Windows;

namespace ZLearn.AdminDesktopApp.Views
{
    public partial class ErrorListWindow : Window
    {
        public List<string> Errors { get; set; }

        public ErrorListWindow(List<string> errors)
        {
            InitializeComponent();
            Errors = errors ?? new List<string>();
            DataContext = this;
        }
    }
}
