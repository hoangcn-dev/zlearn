namespace ZLearn.Application.Temp.Validations
{
    public class PropertyLimits
    {
        public class QuizLimit
        {
            public const int NAME_MAX_LENGTH = 255;
        }

        public class CategoryLimit
        {
            public const int NAME_MAX_LENGTH = 100;
        }

        public class TagLimit
        {
            public const int NAME_MAX_LENGTH = 20;
        }

        public class QuestionLimit
        {
            public const int URL_LENGTH = 2048;
        }

        public class AnswerLimit
        {
            public const int CONTENT_MAX_LENGTH = 255;
            public const int URL_LENGTH = 2048;
        }
    }
}
