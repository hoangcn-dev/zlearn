using System.Windows;
using System.Windows.Controls;
using ZLearn.AdminDesktopApp.Enums;

namespace ZLearn.AdminDesktopApp.Components
{
    public class NavigationButton : Button
    {
        public static readonly DependencyProperty NavDestinationProperty = DependencyProperty.Register(
            nameof(NavDestination),
            typeof(NavDestination),
            typeof(NavigationButton),
            new PropertyMetadata(NavDestination.QuizCate));

        public NavDestination NavDestination
        {
            get => (NavDestination) GetValue(NavDestinationProperty);
            set => SetValue(NavDestinationProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(NavigationButton),
            new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
    }
}
