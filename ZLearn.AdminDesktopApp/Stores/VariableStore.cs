namespace ZLearn.AdminDesktopApp.Stores
{
    public class VariableStore
    {
        private readonly Dictionary<string, object> _var = new();
        public VariableStore() { }

        public bool Add(string key, object value)
        {
            return _var.TryAdd(key, value);
        }

        public bool Remove(string key)
        {
            return _var.Remove(key);
        }

        public void Clear()
        {
            _var.Clear();
        }

        public T? Get<T>(string key, bool clearAfterGet)
        {
            if (_var.TryGetValue(key, out var value) && value is T t)
            {
                if (clearAfterGet) 
                {
                    _var.Remove(key);
                }
                return t;
            }
            return default;
        }

        public void EndSession()
        {
            _var.Remove(Keys.AccessToken);
            _var.Remove(Keys.RefreshToken);
            _var.Remove(Keys.UserName);
            _var.Remove(Keys.Role);
            _var.Remove(Keys.UserId);
            _var.Remove(Keys.ImageUrl);
        }

        public bool ContainsKey(string key)
        {
            return _var.ContainsKey(key);
        }

        public class Keys
        {
            public const string SelectedUserId = nameof(SelectedUserId);

            public const string AccessToken = nameof(AccessToken);
            public const string RefreshToken = nameof(RefreshToken);
            public const string UserName = nameof(UserName);
            public const string Role = nameof(Role);
            public const string UserId = nameof(UserId);
            public const string ImageUrl = nameof(ImageUrl);
        }
    }
}
