using System;
using System.Collections.Generic;

namespace Domain
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<Product> Products { get; set; }
        public Address Address { get; set; }
        public DateTime DateTimeOrdered { get; set; }
    }
}
