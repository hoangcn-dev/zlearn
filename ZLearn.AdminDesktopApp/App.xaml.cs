using Microsoft.Extensions.DependencyInjection;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Services;
using ZLearn.AdminDesktopApp.Features.QuizFeature.ViewModels;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Views;
using ZLearn.AdminDesktopApp.Features.SystemFeature.Services;
using ZLearn.AdminDesktopApp.Features.SystemFeature.ViewModels;
using ZLearn.AdminDesktopApp.Features.SystemFeature.Views;
using ZLearn.AdminDesktopApp.Interceptors;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;
using ZLearn.AdminDesktopApp.Views;

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
            services.AddTransient<AuthInterceptor>();
            services.AddSingleton<IManageWindowService, ManageWindowService>();            
            services.AddHttpClient("", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7284/api/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(60);
            }).AddHttpMessageHandler<AuthInterceptor>();
            services.AddSingleton<IQuizApiService, QuizApiService>();
            services.AddSingleton<IFileApiService, FileApiService>();
            services.AddSingleton<IAuthApiService, AuthApiService>();
            services.AddSingleton<ISystemApiService, SystemApiService>();
            services.AddSingleton<ILogApiService, LogApiService>();
            services.AddSingleton<IUserApiService, UserApiService>();


            // Register viewmodels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<SplashViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<QuizCateViewModel>();
            services.AddTransient<QuizStatViewModel>();
            services.AddTransient<AddQuizCateViewModel>();
            services.AddTransient<UpdateQuizCateViewModel>();
            services.AddTransient<ListQuizViewModel>();
            services.AddTransient<AddQuizViewModel>();

            services.AddTransient<ListUsersViewModel>();
            services.AddTransient<ListLogsViewModel>();
            services.AddTransient<SystemStatViewModel>();
            services.AddTransient<UpdateUserViewModel>();

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
            services.AddTransient<AddQuizCateWindow>(s => new()
            {
                DataContext = s.GetRequiredService<AddQuizCateViewModel>()
            });
            services.AddTransient<UpdateQuizCateWindow>(s => new()
            {
                DataContext = s.GetRequiredService<UpdateQuizCateViewModel>()
            });
            services.AddTransient<ListQuizView>(s => new()
            {
                DataContext = s.GetRequiredService<ListQuizViewModel>()
            });
            services.AddTransient<AddQuizWindow>(s => new()
            {
                DataContext = s.GetRequiredService<AddQuizViewModel>()
            });
            services.AddTransient<ListUsersView>(s => new()
            {
                DataContext = s.GetRequiredService<ListUsersViewModel>()
            });
            services.AddTransient<ListLogsView>(s => new()
            {
                DataContext = s.GetRequiredService<ListLogsViewModel>()
            });
            services.AddTransient<SystemStatView>(s => new()
            {
                DataContext = s.GetRequiredService<SystemStatViewModel>()
            });
            services.AddTransient<UpdateUserWindow>(s => new()
            {
                DataContext = s.GetRequiredService<UpdateUserViewModel>()
            });

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            var windowManager = _serviceProvider.GetRequiredService<IManageWindowService>();
            windowManager.ShowWindow<LoginWindow>();
        }
    }
}
