namespace Domain
{
    public class AuthToken
    {
        public string BearerToken {  get; set; }
        public string RefreshToken {  get; set; }
        public long ExpiresAt {  get; set; }
    }
}
