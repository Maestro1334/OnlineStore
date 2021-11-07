using System;

namespace Domain
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public double Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
