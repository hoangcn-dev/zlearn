using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Application.Temp.Utils
{
    public class Generator
    {
        public static string GenerateId(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Prefix cannot be empty.");
            var guid = Guid.NewGuid().ToString("N").Substring(0, 12); // 12 ký tự
            return $"{prefix}{guid}".ToUpper();
        }
    }
}
