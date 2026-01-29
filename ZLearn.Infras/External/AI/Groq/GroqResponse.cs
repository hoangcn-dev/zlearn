using CloudinaryDotNet.Actions;

namespace ZLearn.Infras.External.AI.Groq
{
    public class GroqResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public string Model { get; set; }
        public List<GroqResponseChoice> Choices { get; set; }
        public Usage Usage { get; set; }
        public object Usage_Breakdown { get; set; }
        public string System_Fingerprint { get; set; }
        public string Service_Tier { get; set; }
    }

    public class GroqResponseChoice
    {
        public int Index { get; set; }
        public GroqResponseMessage Message { get; set; }
        public object Logprobs { get; set; }
        public string Finish_Reason { get; set; }
    }

    public class GroqResponseMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public string Reasoning { get; set; }
    }
}
