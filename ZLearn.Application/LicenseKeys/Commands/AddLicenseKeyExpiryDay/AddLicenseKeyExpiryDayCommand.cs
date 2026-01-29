namespace AddLicenseKeyExpiryDay
{
    public class AddLicenseKeyExpiryDayCommand : IRequest<string>
    {
        public string Id { get; set; }
        public int AddDays { get; set; }
    }
}
