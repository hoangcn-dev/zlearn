using CommunityToolkit.Mvvm.ComponentModel;
using ZLearn.Application.Common.Utils;

namespace ZLearn.AdminDesktopApp.Stores
{
    public class TaskStatusStore : ObservableObject
    {
        private TaskStatus? _currentStatus;
        public TaskStatus? CurrentStatus
        {
            get => _currentStatus;
            set
            {
                if (value is not null && _currentStatus != value)
                {
                    SetProperty(ref _currentStatus, value);
                    History.Add(value);
                }
            }
        }

        private bool _loading;
        public bool Loading
        {
            get => _loading;
            set => SetProperty(ref _loading, value);
        }

        public List<TaskStatus> History { get; } = new List<TaskStatus>();

        public void SetInfoStatus(string value)
        {
            CurrentStatus = new()
            {
                Message = value,
                Type = TaskStatusType.Info,
            };
        }

        public void SetSuccessStatus(string value)
        {
            CurrentStatus = new()
            {
                Message = value,
                Type = TaskStatusType.Success,
            };
        }

        public void SetErrorStatus(string value)
        {
            CurrentStatus = new()
            {
                Message = value,
                Type = TaskStatusType.Error,
            };
        }
    }

    public class TaskStatus
    {
        public string Id => IdGenerator.Generate("LOG");
        public TaskStatusType Type { get; set; }
        public string Message { get; set; }
    }

    public enum TaskStatusType
    {
        Info,
        Error,
        Success
    }

    public static class TaskStatusTypeExtensions
    {
        public static string GetDescription(this TaskStatusType status) => status switch
        {
            TaskStatusType.Info => "INF",
            TaskStatusType.Error => "ERR",
            TaskStatusType.Success => "SUC",
            _ => status.ToString()
        };
    }
}
