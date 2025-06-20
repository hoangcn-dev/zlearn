using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Application.Common.Utils
{
    public abstract class EnvVariableHelper
    {
        public class Names
        {
            public const string POSTGRESQL_CONNECTION_STRING = nameof(POSTGRESQL_CONNECTION_STRING);
        }

        public static string GetValue(string varName)
        {
            return Environment.GetEnvironmentVariable(varName)
                ?? throw new InvalidOperationException($"Required environment variable '{varName}' was not found.");
        }
    }
}
