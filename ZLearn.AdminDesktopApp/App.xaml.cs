using Microsoft.Extensions.DependencyInjection;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;
using ZLearn.AdminDesktopApp.Views;
using ZLearn.AdminDesktopApp.Views.QuizView;

namespace ZLearn.AdminDesktopApp
{
    public partial class App : System.Windows.Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<VariableStore>();
            services.AddSingleton<TaskStatusStore>();
            services.AddSingleton<NavigationStore>();
            services.AddSingleton<IManageWindowService, ManageWindowService>();            
            services.AddHttpClient("", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5074/api/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddSingleton<IQuizApiService, QuizApiService>();


            // Register viewmodels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<SplashViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<QuizCateViewModel>();
            services.AddTransient<QuizStatViewModel>();
            services.AddTransient<AddQuizCategoryViewModel>();
            services.AddTransient<UpdateQuizCategoryViewModel>();

            // Register views
            services.AddSingleton<MainWindow>(s => new()
            {
                DataContext = s.GetRequiredService<MainViewModel>()
            });
            services.AddTransient<SplashWindow>(s => new()
            {
                DataContext = s.GetRequiredService<SplashViewModel>()
            });
            services.AddTransient<LoginWindow>(s => new()
            {
                DataContext = s.GetRequiredService<LoginViewModel>()
            });
            services.AddTransient<QuizCateView>(s => new()
            {
                DataContext = s.GetRequiredService<QuizCateViewModel>()
            });
            services.AddTransient<QuizStatView>(s => new()
            {
                DataContext = s.GetRequiredService<QuizStatViewModel>()
            });
            services.AddTransient<AddQuizCategoryWindow>(s => new()
            {
                DataContext = s.GetRequiredService<AddQuizCategoryViewModel>()
            });
            services.AddTransient<UpdateQuizCategoryWindow>(s => new()
            {
                DataContext = s.GetRequiredService<UpdateQuizCategoryViewModel>()
            });

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);

            var navigation = _serviceProvider.GetRequiredService<NavigationStore>();
            navigation.CurrentViewModel = _serviceProvider.GetRequiredService<QuizCateViewModel>();
            navigation.CurrentDestination = Enums.NavDestination.QuizCate;

            var windowManager = _serviceProvider.GetRequiredService<IManageWindowService>();
            windowManager.ShowWindow<MainWindow>();
        }
    }
}
