using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Models;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Services;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Views;
using ZLearn.AdminDesktopApp.Helpers;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.AdminDesktopApp.Features.QuizFeature.ViewModels
{
    public partial class AddQuizViewModel : ViewModelBase
    {
        private string _uploadFilePath = string.Empty;
        private readonly IQuizApiService _quizApiService;
        private readonly IFileApiService _fileApiService;
        private readonly IManageWindowService _manageWindowService;

        [ObservableProperty]
        private string name;
        [ObservableProperty]
        private CateListItemDto? selectedCategory = null;
        

        public string NewTag { get; set; } = string.Empty;
        public ObservableCollection<UploadFileStatus> UploadFiles { get; } = new();
        public ObservableCollection<CateListItemDto> Categories { get; } = new();
        public ObservableCollection<string> DefaultTags { get; } = new();
        public ObservableCollection<string> Tags { get; set; } = new();
        public ObservableCollection<CreateQuestionDto> Questions { get; set; } = new();


        public bool IsLoading => _taskStatusStore.Loading;
        public string UploadFileName => $"{Path.GetFileName(_uploadFilePath)} - {Questions.Count} questions";
        public Visibility ShowFileUploadedMessage => !string.IsNullOrEmpty(_uploadFilePath) ? Visibility.Visible : Visibility.Collapsed;
        
        
        public ICommand UploadQuestionDataFileCommand { get; }
        public IRelayCommand UploadMediaFileCommand { get; }
        public IRelayCommand AddTagCommand { get; }
        public ICommand RemoveTagCommand { get; }
        public IRelayCommand SaveQuizCommand { get; }


        public AddQuizViewModel(
            TaskStatusStore taskStatusStore,
            VariableStore store,
            IQuizApiService quizApiService,
            IFileApiService fileApiService,
            IManageWindowService manageWindowService) : base(taskStatusStore, store)
        {
            _quizApiService = quizApiService;
            _fileApiService = fileApiService;
            _taskStatusStore.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TaskStatusStore.Loading))
                {
                    OnPropertyChanged(nameof(IsLoading));
                }
            };

            UploadQuestionDataFileCommand = new RelayCommand(BrowserJsonFile);
            UploadMediaFileCommand = new RelayCommand(BrowserMediaFile, () => UploadFiles.Count > 0);
            AddTagCommand = new RelayCommand(AddTag);
            RemoveTagCommand = new RelayCommand<string>(tag =>
            {
                DialogHelper.ShowConfirm($"Bạn có chắc muốn xóa thẻ '{tag}' không?", () =>
                {
                    if (Tags.Contains(tag))
                    {
                        Tags.Remove(tag);
                    }
                });
            });
            SaveQuizCommand = new RelayCommand(SaveQuiz);

            LoadCategoriesData();
            LoadTagsData();
            _manageWindowService = manageWindowService;
        }

        public async void SaveQuiz()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                DialogHelper.ShowErrorMess("Vui lòng nhập tên quiz.");
                return;
            }
            if (SelectedCategory is null)
            {
                DialogHelper.ShowErrorMess("Vui lòng chọn danh mục cho quiz.");
                return;
            }
            if (Tags.Count == 0)
            {
                DialogHelper.ShowErrorMess("Vui lòng thêm ít nhất một thẻ cho quiz.");
                return;
            }
            if (Questions.Count == 0)
            {
                DialogHelper.ShowErrorMess("Vui lòng thêm câu hỏi vào quiz.");
                return;
            }
            if (UploadFiles.Any(uf => !uf.IsUploaded))
            {
                DialogHelper.ShowErrorMess("Vui lòng tải lên hết file đính kèm.");
                return;
            }
            if (UploadFiles.Count > 0)
            {
                var savedFileNames = await SaveMediaFile();
                if (savedFileNames is null)                 
                {
                    DialogHelper.ShowErrorMess("Lỗi khi lưu tệp đính kèm.");
                    return;
                }

                // Mark files as saved
                foreach (var file in UploadFiles)
                {
                    if (savedFileNames.ContainsKey(file.FileName))
                    {
                        file.IsSaved = true;
                    }
                }
                foreach (var question in Questions)
                {
                    question.ReplaceAllFileNameToFileId(savedFileNames);
                }
            }

            var quiz = new CreateQuizDto
            {
                Name = Name,
                CategoryId = SelectedCategory.Id,
                Tags = Tags.ToList(),
                Questions = Questions.ToList()
            };
            var result = await ExecuteAsync(() => _quizApiService.CreateNewQuizAsync(quiz));
            if (result is not null && result.Succeeded)
            {
                DialogHelper.ShowSuccessMess("Tạo quiz thành công!");
                _manageWindowService.CloseWindow<AddQuizWindow>();
                return;
            }
            DialogHelper.ShowErrorMess("Đã có lỗi xảy ra khi tạo quiz: " + result?.Message);
        }


        public async Task<Dictionary<string, string>?> SaveMediaFile()
        {
            var filePaths = UploadFiles
                .Where(f => f.IsUploaded && !f.IsSaved)
                .Select(f => f.FilePath).ToList();
            if (filePaths.Count == 0) return new();
            var result = await ExecuteAsync(() => _fileApiService.SaveFilesAsync(filePaths));
            if (result is not null && result.Succeeded)
            {
                var savedFiles = result.Data.Files.ToDictionary(f => f.FileName, f => f.Id);
                return savedFiles;
            }
            return null;
        }

        private void AddTag()
        {
            if (string.IsNullOrWhiteSpace(NewTag))
            {
                DialogHelper.ShowErrorMess("Vui lòng nhập tên thẻ.");
                return;
            }
            if (Tags.Contains(NewTag))
            {
                DialogHelper.ShowErrorMess("Thẻ này đã tồn tại.");
                return;
            }
            Tags.Add(NewTag);
            NewTag = string.Empty;
            OnPropertyChanged(NewTag);
        }

        public async void LoadCategoriesData()
        {
            var categoriesResult = await ExecuteAsync(() => _quizApiService.GetAllCategoriesAsync());
            if (categoriesResult is not null && categoriesResult.Succeeded)
            {
                Categories.Clear();
                foreach (var cate in categoriesResult.Data)
                {
                    Categories.Add(cate);
                }
            }
        }

        public async void LoadTagsData()
        {
            var tags = await ExecuteAsync(() => _quizApiService.GetAllTagsAsync());
            if (tags is not null && tags.Succeeded)
            {
                DefaultTags.Clear();
                foreach (var tag in tags.Data)
                {
                    DefaultTags.Add(tag);
                }
            }
        }

        private void BrowserMediaFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.svg)|*.png;*.jpg;*.jpeg;*.gif;*.svg|"
                      + "Audio files (*.mp3;*.wav;*.ogg)|*.mp3;*.wav;*.ogg|"
                      + "Video files (*.mp4)|*.mp4",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Multiselect = true
            };
            if (dialog.ShowDialog() == true)
            {
                foreach (var filePath in dialog.FileNames)
                {
                    var fileName = Path.GetFileName(filePath);
                    var uploadedFile = UploadFiles.FirstOrDefault(f => f.FileName == fileName);
                    if (uploadedFile is null)
                    {
                        DialogHelper.ShowErrorMess($"Tên tệp {fileName} không hợp lệ.");
                        return;
                    }

                    if (uploadedFile.IsUploaded)
                    {
                        DialogHelper.ShowConfirm($"Tệp {fileName} đã được tải lên trước đó, có muốn thay thế không?", () =>
                        {
                            uploadedFile.FilePath = filePath;
                        });
                        return;
                    }

                    uploadedFile.FilePath = filePath;
                    uploadedFile.IsUploaded = true;
                    uploadedFile.IsSaved = false;
                }
                    
            }
        }

        private void BrowserJsonFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            };
            if (dialog.ShowDialog() == true)
            {
                var jsonData = File.ReadAllText(dialog.FileName);
                UploadFiles.Clear();
                if (ValidateQuestionJsonData(jsonData))
                {
                    _uploadFilePath = dialog.FileName;
                    OnPropertyChanged(nameof(UploadFileName));
                    OnPropertyChanged(nameof(ShowFileUploadedMessage));
                }
            }
        }

        private bool ValidateQuestionJsonData(string jsonData)
        {
            try
            {
                var errors = new List<string>();
                var data = JsonSerializer.Deserialize<List<CreateQuestionDto>>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (data.Count == 0)
                    errors.Add("Không chứa câu hỏi nào.");
                for (int i = 0; i < data.Count; i++)
                {
                    var question = data[i];
                    if (string.IsNullOrEmpty(question.StringContent) && question.MediaFileIds.Count == 0)
                        errors.Add($"Câu hỏi {i + 1}: Không có nội dung hoặc tệp đính kèm nào.");
                    if (question.Answers.Count < 2)
                        errors.Add($"Câu hỏi {i + 1}: Ít hơn 2 đáp án.");
                    if (!question.Answers.Any(a => a.Key == question.CorrectKey))
                        errors.Add($"Câu hỏi {i + 1}: Key đáp án không hợp lệ.");
                    if (question.Answers.Any(a => a.MediaFileIds.Count == 0 && string.IsNullOrEmpty(a.StringContent)))
                        errors.Add($"Câu hỏi {i + 1}: Tồn tại đáp án không có nội dung hoặc tệp đính kèm nào.");
                }

                if (errors.Count == 0)
                {
                    Questions.Clear();
                    foreach (var question in data)
                    {
                        Questions.Add(question);
                        foreach (var file in question.GetAllFilesInfo())
                        {
                            UploadFiles.Add(new()
                            {
                                FileName = file,
                                IsUploaded = false
                            });
                        }
                    }
                    UploadMediaFileCommand.NotifyCanExecuteChanged();
                    return true;
                }

                DialogHelper.ShowErrors(errors);
                return false;
            }
            catch (JsonException ex)
            {
                DialogHelper.ShowErrorMess("Dữ liệu không phải là JSON hợp lệ: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                DialogHelper.ShowErrorMess("Đã có lỗi xảy ra trong quá trình xác thực dữ liệu: " + ex.Message);
                return false;
            }
        }
    }
}