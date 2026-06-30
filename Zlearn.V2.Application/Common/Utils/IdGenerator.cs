namespace Zlearn.V2.Application.Common.Utils
{
    public abstract class IdGenerator
    {
        public static string Generate(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Prefix cannot be empty.");
            if (prefix.Length != 3)
                throw new ArgumentException("Prefix length must be 3.");
            var guid = Guid.NewGuid().ToString("N")[..12];
            return $"{prefix}{guid}".ToUpper();
        }
    }
}
