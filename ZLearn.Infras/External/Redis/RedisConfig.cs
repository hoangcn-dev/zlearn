using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Infras.External.Redis
{
    public class RedisConfig
    {
        public RedisEndPoints EndPoints { get; set; }
        public bool Ssl { get; set; }
        public int ConnectTimeout { get; set; }
        public int SyncTimeout { get; set; }
        public int ConnectRetry { get; set; }
    }

    public class RedisEndPoints
    {
        public string Default { get; set; }
    }
}
