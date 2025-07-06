using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;

namespace ZLearn.AdminDesktopApp.Features.QuizFeature.ViewModels
{
    public class QuizStatViewModel : ViewModelBase
    {
        public QuizStatViewModel(
            VariableStore store,
            TaskStatusStore taskStatusStore) : base(taskStatusStore, store)
        {
        }
    }
}
