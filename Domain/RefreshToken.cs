using System;

namespace Domain
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId {  get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
