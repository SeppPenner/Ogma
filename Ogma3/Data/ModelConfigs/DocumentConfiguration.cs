using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ogma3.Data.Models;
using Ogma3.Infrastructure;

namespace Ogma3.Data.ModelConfigs
{
    public class DocumentConfiguration : BaseConfiguration<Document>
    {
        public override void Configure(EntityTypeBuilder<Document> builder)
        {
            base.Configure(builder);
            
            // CONSTRAINTS
            builder
                .Property(d => d.Title)
                .IsRequired();

            builder
                .Property(d => d.Slug)
                .IsRequired();

            builder
                .Property(d => d.RevisionDate)
                .HasDefaultValue(null);

            builder
                .Property(d => d.CreationTime)
                .IsRequired()
                .HasDefaultValueSql(PgConstants.CurrentTimestamp);

            builder
                .Property(d => d.Version)
                .IsRequired()
                .HasDefaultValue(1);

            builder
                .Property(d => d.Body)
                .IsRequired();

            // NAVIGATION
            builder
                .HasIndex(d => new { d.Slug, d.Version })
                .IsUnique();
            
            builder
                .HasIndex(d => new { d.Title, d.Version })
                .IsUnique();
        }
    }
}