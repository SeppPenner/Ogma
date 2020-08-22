﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ogma3.Data.Enums;
using Ogma3.Data.Models;

namespace Ogma3.Data
{
    public class ApplicationDbContext : IdentityDbContext<OgmaUser, Role, long>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<EStoryStatus>();
        }

        
        public DbSet<Tag> Tags { get; set; }
        public DbSet<StoryTag> StoryTags { get; set; }
        public DbSet<Namespace> Namespaces { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<ChaptersRead> ChaptersRead { get; set; }
        public DbSet<CommentsThread> CommentThreads { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<ShelfStory> ShelfStories { get; set; }
        public DbSet<Blogpost> Blogposts { get; set; }
        
        // Clubs
        public DbSet<Club> Clubs { get; set; }
        public DbSet<ClubMember> ClubMembers { get; set; }
        public DbSet<ClubThread> ClubThreads { get; set; }
        public DbSet<ClubThreadComment> ClubThreadComments { get; set; }
        
        
        // Secondary
        public DbSet<Document> Documents { get; set; }
        public DbSet<Icon> Icons { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        
        // Invite codes
        public DbSet<InviteCode> InviteCodes { get; set; }

        
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Extensions
            builder.HasPostgresExtension("uuid-ossp");
            
            
            // User
            builder.Entity<OgmaUser>()
                .Ignore(u => u.PhoneNumber)
                .Ignore(u => u.PhoneNumberConfirmed);
            builder.Entity<OgmaUser>()
                .HasOne(u => u.CommentsThread)
                .WithOne(ct => ct.User)
                .HasForeignKey<CommentsThread>(ct => ct.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Tag
            builder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();
            builder.Entity<Tag>()
                .HasOne(t => t.Namespace)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);

            // Namespace
            builder.Entity<Namespace>()
                .HasIndex(n => n.Name)
                .IsUnique();
            
            // Rating
            builder.Entity<Rating>()
                .HasIndex(r => r.Name)
                .IsUnique();
            
            // Story
            builder.Entity<Story>()
                .Property(s => s.Id)
                .ValueGeneratedOnAdd();
            builder.Entity<Story>()
                .Property(p => p.IsPublished)
                .HasDefaultValue(false);
            builder.Entity<Story>()
                .HasOne(s => s.Rating)
                .WithMany();
            builder.Entity<Story>()
                .HasMany(s => s.Chapters)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Story>()
                .HasOne(s => s.Author)
                .WithMany();
            builder.Entity<Story>()
                .HasMany(s => s.Votes)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            
            // Chapter
            builder.Entity<Chapter>()
                .Property(p => p.IsPublished)
                .HasDefaultValue(false);
            builder.Entity<Chapter>()
                .HasOne(c => c.CommentsThread)
                .WithOne(ct => ct.Chapter)
                .HasForeignKey<CommentsThread>(ct => ct.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Chapter reads
            builder.Entity<ChaptersRead>(ent =>
            {
                ent.HasOne(cr => cr.Story)
                    .WithMany();
                ent.HasOne(cr => cr.User)
                    .WithMany();
            });

            // Story tags
            builder.Entity<StoryTag>()
                .HasKey(st => new {st.StoryId, st.TagId});
            builder.Entity<StoryTag>()
                .HasOne(st => st.Story)
                .WithMany(s => s.StoryTags);
            builder.Entity<StoryTag>()
                .HasOne(st => st.Tag)
                .WithMany();
            
            // Comment threads
            builder.Entity<CommentsThread>()
                .HasMany(ct => ct.Comments)
                .WithOne()
                .HasForeignKey(c => c.CommentsThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comments
            builder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany();

            
            // Votes
            builder.Entity<Vote>()
                .HasOne(v => v.User)
                .WithMany();
            builder.Entity<Vote>()
                .HasIndex(v => new {v.UserId, v.StoryId})
                .IsUnique();
            
            // Shelf stories
            builder.Entity<ShelfStory>()
                .HasKey(ss => new {ss.ShelfId, ss.StoryId});
            builder.Entity<ShelfStory>()
                .HasOne(ss => ss.Shelf)
                .WithMany(s => s.ShelfStories)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ShelfStory>()
                .HasOne(ss => ss.Story)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            // Shelves
            builder.Entity<Shelf>()
                .HasOne(s => s.Icon)
                .WithMany();
            
            // Blogposts
            builder.Entity<Blogpost>()
                .HasOne(b => b.Author)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Blogpost>()
                .HasOne(b => b.CommentsThread)
                .WithOne(ct => ct.Blogpost)
                .HasForeignKey<CommentsThread>(ct => ct.BlogpostId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Clubs
            builder.Entity<Club>(ent =>
            {
                ent.HasMany(c => c.Threads)
                    .WithOne()
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Club members
            builder.Entity<ClubMember>(ent =>
            {
                ent.HasKey(cm => new {cm.ClubId, cm.MemberId});
                ent.HasOne(cm => cm.Club)
                    .WithMany(c => c.ClubMembers)
                    .OnDelete(DeleteBehavior.Cascade);
                ent.HasOne(cm => cm.Member)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Club threads
            builder.Entity<ClubThread>(ent =>
            {
                ent.HasOne(ct => ct.Author)
                    .WithMany()
                    .OnDelete(DeleteBehavior.SetNull);
                ent.HasMany(ct => ct.Comments)
                    .WithOne()
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Club thread comments
            builder.Entity<ClubThreadComment>(ent =>
            {
                ent.HasOne(ctc => ctc.Author)
                    .WithMany()
                    .OnDelete(DeleteBehavior.SetNull);
            });

            
            // Enums
            builder.HasPostgresEnum<EStoryStatus>();
            
            
            
            // Documents
            builder.Entity<Document>()
                .HasIndex(d => d.Slug)
                .IsUnique();
            
            // Invite codes
            builder.Entity<InviteCode>()
                .HasOne(c => c.UsedBy)
                .WithOne()
                .HasForeignKey<InviteCode>(c => c.UsedById)
                .OnDelete(DeleteBehavior.Cascade);
            
        }
        
        
    }
}
