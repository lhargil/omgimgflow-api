using API.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configurations
{
    public class OmgImageConfiguration: BaseEntityTypeConfiguration<OmgImage>
    {
        public override void Configure(EntityTypeBuilder<OmgImage> builder)
        {
            builder.Property(p => p.Id).ValueGeneratedOnAdd();
            builder
                .Property(b => b.Filename)
                .IsRequired();

            base.Configure(builder);
        }
    }

    public class OmgImageTagConfiguration: BaseEntityTypeConfiguration<OmgImageTag>
    {
        public override void Configure(EntityTypeBuilder<OmgImageTag> builder)
        {
            builder.Property(p => p.Id).ValueGeneratedOnAdd();
            base.Configure(builder);
        }
    }
}
