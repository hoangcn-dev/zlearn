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

        public T? Get<T>(string key)
        {
            if (_var.TryGetValue(key, out var value) && value is T t)
            {
                _var.Remove(key);
                return t;
            }
            return default;
        }
    }
}
