﻿using Infrastructure.Data.Postgres.Entities.Base;

namespace Infrastructure.Data.Postgres.Entities
{
    public class ProductSale : TrackedBaseEntity<int>
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public Product Product { get; set; } = null!;
    }
}
