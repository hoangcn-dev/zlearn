using System.Windows;
using System.Windows.Controls;
using ZLearn.AdminDesktopApp.Components;
using ZLearn.AdminDesktopApp.ViewModels;

namespace ZLearn.AdminDesktopApp
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<Button, StackPanel> _menuManager = new();
        public MainWindow()
        {
            InitializeComponent();
            _menuManager.Add(btnQuizTest, spQuizTestItem);
            _menuManager.Add(btnSystem, spSystem);
            Loaded += (s, e) => InitOriginState();
        }

        private void OnMenuButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && _menuManager.TryGetValue(btn, out StackPanel? sp))
            {
                if (sp.Visibility == Visibility.Visible) sp.Visibility = Visibility.Collapsed;
                else sp.Visibility = Visibility.Visible;
            }
        }

        private void OnNavigate(object sender, RoutedEventArgs e)
        {
            if (sender is NavigationButton btn)
            {
                // Unselected current button
                var currentSelectedBtn = _menuManager.Values
                    .SelectMany(sp => sp.Children.OfType<NavigationButton>())
                    .FirstOrDefault(b => b.IsSelected);
                if (currentSelectedBtn != null)
                    currentSelectedBtn.IsSelected = false;

                // Collapse all submenus not contain the btn
                foreach (var sp in _menuManager.Values)
                {
                    if (sp.Children.Contains(btn)) sp.Visibility = Visibility.Visible;
                    else sp.Visibility = Visibility.Collapsed;
                }

                btn.IsSelected = true;
            }
        }

        private void InitOriginState()
        {
            var vm = DataContext as MainViewModel;
            foreach (var sp in _menuManager.Values)
            {
                foreach (var btn in sp.Children.OfType<NavigationButton>())
                {
                    if (btn.NavDestination == vm?.CurrentDestination)
                    {
                        sp.Visibility = Visibility.Visible;
                        btn.IsSelected = true;
                        return;
                    }
                }
            }
        }
    }
}