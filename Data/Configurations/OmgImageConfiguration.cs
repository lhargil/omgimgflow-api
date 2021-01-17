using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configurations
{
    public class OmgImageConfiguration: BaseEntityTypeConfiguration<OmgImage>
    {
        public override void Configure(EntityTypeBuilder<OmgImage> builder)
        {
            builder
                .Property(p => p.Id).ValueGeneratedOnAdd();
            
            builder
                .Property(p => p.Filename)
                .IsRequired();

            builder
                .Property(p => p.Title)
                .IsRequired();

            builder
                .HasMany(p => p.Tags)
                .WithOne();

            builder
                .HasQueryFilter(p => EF.Property<bool>(p, "IsDeleted") == false);

            base.Configure(builder);
        }
    }

    public class OmgImageTagConfiguration: BaseEntityTypeConfiguration<OmgImageTag>
    {
        public override void Configure(EntityTypeBuilder<OmgImageTag> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
            
            builder
                .HasQueryFilter(p => EF.Property<bool>(p, "IsDeleted") == false);

            base.Configure(builder);
        }
    }
}
