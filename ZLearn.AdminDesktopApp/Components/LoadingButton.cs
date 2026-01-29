using System.Windows;
using System.Windows.Controls;

namespace ZLearn.AdminDesktopApp.Components
{
    public class LoadingButton : Button
    {
        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(LoadingButton),
            new PropertyMetadata(false));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }
    }
}
