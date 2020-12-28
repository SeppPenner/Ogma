using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ogma3.Data.Models;
using Ogma3.Infrastructure;

namespace Ogma3.Data.ModelConfigs
{
    public class BlogpostConfiguration : BaseConfiguration<Blogpost>
    {
        public override void Configure(EntityTypeBuilder<Blogpost> builder)
        {
            base.Configure(builder);
            
            // CONSTRAINTS
            builder
                .Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(CTConfig.CBlogpost.MaxTitleLength);
            
            builder
                .Property(b => b.Slug)
                .IsRequired()
                .HasMaxLength(CTConfig.CBlogpost.MaxTitleLength);

            builder
                .Property(b => b.PublishDate)
                .IsRequired()
                .HasDefaultValueSql(PgConstants.CurrentTimestamp);

            builder
                .Property(b => b.IsPublished)
                .HasDefaultValue(false);

            builder
                .Property(b => b.Body)
                .IsRequired()
                .HasMaxLength(CTConfig.CBlogpost.MaxBodyLength);

            builder
                .Property(b => b.WordCount)
                .HasDefaultValue(0);

            builder
                .Property(b => b.Hashtags)
                .IsRequired()
                .HasMaxLength(CTConfig.CBlogpost.MaxTagsAmount)
                .HasDefaultValue(Array.Empty<string>());
            
            
            // NAVIGATION
            builder
                .HasOne(b => b.Author)
                .WithMany(u => u.Blogposts)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder
                .HasOne(b => b.CommentsThread)
                .WithOne(ct => ct.Blogpost)
                .HasForeignKey<CommentsThread>(ct => ct.BlogpostId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder
                .HasOne(b => b.AttachedStory)
                .WithMany()
                .HasForeignKey(b => b.AttachedStoryId)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder
                .HasOne(b => b.AttachedChapter)
                .WithMany()
                .HasForeignKey(b => b.AttachedChapterId)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder
                .HasOne(b => b.ContentBlock)
                .WithOne()
                .HasForeignKey<Blogpost>(b => b.ContentBlockId);
            
            builder
                .HasMany(b => b.Reports)
                .WithOne(r => r.Blogpost)
                .HasForeignKey(r => r.BlogpostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}