using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlearn.V2.Application.Common.Utils
{
    public abstract class EnvVariableHelper
    {
        public static string GetValue(string varName)
        {
            return Environment.GetEnvironmentVariable(varName)
                ?? throw new InvalidOperationException($"Required environment variable '{varName}' was not found.");
        }

        public static string GetValueOrDefault(string varName, string defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(varName);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }
    }

    public class EnvVariableNames
    {
        public const string ADMIN_PASSWORD = nameof(ADMIN_PASSWORD);
        public const string CONNECTION_STRING_POSTGRES = nameof(CONNECTION_STRING_POSTGRES);
        public const string CONNECTION_STRING_REDIS = nameof(CONNECTION_STRING_REDIS);
        public const string JWT_SECRET_KEY = nameof(JWT_SECRET_KEY);
        public const string GOOGLE_CLIENT_ID = nameof(GOOGLE_CLIENT_ID);
        public const string GOOGLE_CLIENT_SECRET = nameof(GOOGLE_CLIENT_SECRET);
        public const string CLOUDINARY_NAME = nameof(CLOUDINARY_NAME);
        public const string CLOUDINARY_API_KEY = nameof(CLOUDINARY_API_KEY);
        public const string CLOUDINARY_API_SECRET = nameof(CLOUDINARY_API_SECRET);
        public const string GROQ_API_KEY = nameof(GROQ_API_KEY);
        public const string CONNECTION_STRING_MONGODB = nameof(CONNECTION_STRING_MONGODB);
        public const string MONGODB_DATABASE_NAME = nameof(MONGODB_DATABASE_NAME);
        public const string RABBITMQ_HOST = nameof(RABBITMQ_HOST);
        public const string RABBITMQ_PORT = nameof(RABBITMQ_PORT);
        public const string RABBITMQ_USERNAME = nameof(RABBITMQ_USERNAME);
        public const string RABBITMQ_PASSWORD = nameof(RABBITMQ_PASSWORD);
    }

}

