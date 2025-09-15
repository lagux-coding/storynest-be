using Microsoft.EntityFrameworkCore;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence
{
    public class MyDbContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // Plans & Subscriptions
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }

        // AI / Credits
        public DbSet<AICredit> AICredits { get; set; }
        public DbSet<AITransaction> AITransactions { get; set; }
        public DbSet<AIUsageLog> AIUsageLogs { get; set; }

        // Stories & Content
        public DbSet<Story> Stories { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<CollectionStory> CollectionStories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<StoryTag> StoryTags { get; set; }
        public DbSet<StoryView> StoryViews { get; set; }
        public DbSet<UserReport> UserReports { get; set; }
        public DbSet<YearlyMemory> YearlyMemories { get; set; }

        // Notifications
        public DbSet<Notification> Notifications { get; set; }

        // Ads
        public DbSet<Advertisement> Advertisements { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
