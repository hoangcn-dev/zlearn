namespace ZLearn.Infras.External.AI.Groq
{
    public class GroqRequest
    {
        public string model { get; set; }
        public List<GroqRequestMessage> messages { get; set; }
    }

    public class GroqRequestMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}
