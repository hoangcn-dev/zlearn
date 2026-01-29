using CommunityToolkit.Mvvm.ComponentModel;
namespace ZLearn.AdminDesktopApp.Features.QuizFeature.Models
{
    public partial class UploadFileStatus : ObservableObject
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        [ObservableProperty]
        private bool isUploaded;

        [ObservableProperty]
        private bool isSaved;
    }
}
