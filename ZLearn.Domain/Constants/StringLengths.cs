using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Domain.Constants
{
    public abstract class StringLengths
    {
        public const int QuizNameMaxLength = 255;
        public const int QuizNameMinLength = 2;

        public const int CateNameMaxLength = 50;
        public const int CateNameMinLength = 2;

        public const int TagNameMaxLength = 20;
        public const int TagNameMinLength = 2;

        public const int UrlMaxLength = 2048;
        public const int IdMaxLength = 20;
    }
}
