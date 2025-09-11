using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Infras.Services.Identity
{
    public class JwtConfig
    {
        public int ATExpirationMinutes { get; set; }
        public int RTExpirationMinutes { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
