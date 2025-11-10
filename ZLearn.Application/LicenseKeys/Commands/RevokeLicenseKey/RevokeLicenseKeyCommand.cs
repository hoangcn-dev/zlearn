namespace RevokeLicenseKey
{
    public class RevokeLicenseKeyCommand : IRequest<string>
    {
        public string Id { get; set; }
    }
}
