using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Application.Common.Utils
{
    public abstract class EnvVariableHelper
    {
        public static string GetValue(string varName)
        {
            return Environment.GetEnvironmentVariable(varName)
                ?? throw new InvalidOperationException($"Required environment variable '{varName}' was not found.");
        }
    }

    public class EnvVariableNames
    {
        public const string POSTGRESQL_CONNECTION_STRING = nameof(POSTGRESQL_CONNECTION_STRING);
        public const string JWT_SECRET_KEY = nameof(JWT_SECRET_KEY);
        public const string REDIS_CONNECTION_PASSWORD = nameof(REDIS_CONNECTION_PASSWORD);
        public const string GOOGLE_CLIENT_ID = nameof(GOOGLE_CLIENT_ID);
        public const string GOOGLE_CLIENT_SECRET = nameof(GOOGLE_CLIENT_SECRET);
    }

}
