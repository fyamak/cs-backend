﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data.Postgres.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Postgres.EntityFramework.Configurations
{
    public class ProductSupplyConfiguration : IEntityTypeConfiguration<ProductSupply>
    {
        public void Configure(EntityTypeBuilder<ProductSupply> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProductId).IsRequired();
            builder.Property(x => x.Quantity).IsRequired();
            builder.Property(x => x.Date).IsRequired();
            builder.Property(x => x.RemainingQuantity).IsRequired();
            builder.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
            
            builder.HasOne(x => x.Product)
                .WithMany(x => x.Supplies)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
