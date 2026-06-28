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
        public const string EXAM_PARTICIPANT_MAP = nameof(EXAM_PARTICIPANT_MAP);
        public const string EXAM_SESSION_DISCONNECT = nameof(EXAM_SESSION_DISCONNECT);
        public const string EXAM_SESSION = nameof(EXAM_SESSION);
        public const string EXAM_USER_SESSION = nameof(EXAM_USER_SESSION);
    }
}
