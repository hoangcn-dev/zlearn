using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Infra.Data.Utils
{
    public class EnvVariableHelper
    {
        /// <summary>
        /// Gets an environment variable or throws an appropriate exception if not found.
        /// </summary>
        /// <param name="name">The name of the environment variable</param>
        /// <returns>The value of the environment variable</returns>
        /// <exception cref="InvalidOperationException">Thrown when the environment variable is not found</exception>
        public static string GetVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name)
                ?? throw new InvalidOperationException($"Required environment variable '{name}' was not found.");
        }
    }
}
