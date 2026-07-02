namespace Zlearn.V2.Application.Identity.DTOs
{
    public class JwtConfig
    {
        public int ATExpirationMinutes { get; set; }
        public int RTExpirationMinutes { get; set; }
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
