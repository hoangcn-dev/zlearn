namespace ValidateLicenseKey
{
    public class ValidateLicenseKeyCommand : IRequest<TimeSpan>
    {
        public string HardwareId { get; set; }
        public string Key { get; set; }

    }
}
