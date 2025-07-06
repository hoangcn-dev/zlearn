using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Application.Auth.DTOs
{
    public class SignInRequestDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
