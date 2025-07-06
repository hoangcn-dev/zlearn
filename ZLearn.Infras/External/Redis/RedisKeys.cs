using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Infras.External.Redis
{
    public class RedisKeys
    {
        public const string ACCESS_TOKEN = nameof(ACCESS_TOKEN);
        public const string REFRESH_TOKEN = nameof(REFRESH_TOKEN);
        public const string REVOKED_ACCESS_TOKEN = nameof(REVOKED_ACCESS_TOKEN);
    }
}
