using ZLearn.AdminDesktopApp.Stores;

namespace ZLearn.AdminDesktopApp.ViewModels
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
