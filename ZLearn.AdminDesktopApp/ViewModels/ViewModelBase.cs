using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.AdminDesktopApp.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        protected readonly TaskStatusStore _taskStatusStore;
        protected readonly VariableStore _store;

        protected ViewModelBase(
            TaskStatusStore taskStatusStore, 
            VariableStore store)
        {
            _taskStatusStore = taskStatusStore;
            _store = store;
        }

        protected Result<T>? Execute<T>(Func<Result<T>> action)
        {
            _taskStatusStore.Loading = true;
            try
            {
                var res = action();
                _taskStatusStore.SetSuccessStatus(res.Message ?? "Thực hiện tác vụ thành công");
                return res;
            }
            catch (Exception ex)
            {
                _taskStatusStore.SetErrorStatus(ex.Message);
                return Result<T>.Failure("Đã có lỗi xảy ra");
            }
            finally
            {
                _taskStatusStore.Loading = false;
            }
        }

        protected async Task<Result<T>?> ExecuteAsync<T>(Func<Task<Result<T>>> action)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                _taskStatusStore.Loading = true;
            });

            try
            {
                var res = await Task.Run(async () => await action());
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _taskStatusStore.SetSuccessStatus(res.Message ?? "Thực hiện tác vụ thành công");
                    _taskStatusStore.Loading = false;
                });
                return res;
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _taskStatusStore.SetErrorStatus(ex.Message);
                    _taskStatusStore.Loading = false;
                });
                return Result<T>.Failure("Đã có lỗi xảy ra");
            }
            finally
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _taskStatusStore.Loading = false;
                    Mouse.OverrideCursor = null;
                });
            }
        }

        public virtual void Dispose() { }
    }
}
