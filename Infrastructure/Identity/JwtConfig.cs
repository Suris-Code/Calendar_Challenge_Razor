namespace Infrastructure.Identity
{
    public class JwtConfig 
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audiencie { get; set; }
        public int ExpireMinutes { get; set; }
    }
}
