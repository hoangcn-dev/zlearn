using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ZLearn.AdminDesktopApp.Models
{
    public class DataGridItem<T> : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int Index { get; set; }
        public T Data { get; set; }
        public bool IsSelected 
        { 
            get => _isSelected; 
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public static ObservableCollection<DataGridItem<T>> MapFromList(IEnumerable<T> items)
        {
            var res = new ObservableCollection<DataGridItem<T>>();
            for (int i = 0; i < items.Count(); i++)
            {
                res.Add(new DataGridItem<T>
                {
                    Index = i + 1,
                    Data = items.ElementAt(i),
                    IsSelected = false,
                });
            }
            return res;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
