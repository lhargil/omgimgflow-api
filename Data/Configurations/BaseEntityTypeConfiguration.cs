using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data.Configurations
{
    public abstract class BaseEntityTypeConfiguration<T> : IEntityTypeConfiguration<T> where T : class
    {
        public virtual void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> builder)
        {
            builder.Property<DateTime>("DateCreated")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property<DateTime>("DateModified")
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property<Boolean>("IsDeleted")
                .IsRequired()
                .HasDefaultValue(false);
            builder.Property<byte[]>("Version")
                .ValueGeneratedOnAdd()
                .IsRowVersion();
        }
    }
}
