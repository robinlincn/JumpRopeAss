using JumpRopeAss.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Infrastructure;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();
    public DbSet<UserIdentitySubmit> UserIdentitySubmits => Set<UserIdentitySubmit>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventEntry> EventEntries => Set<EventEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NewsArticle>().ToTable("news_article");
        modelBuilder.Entity<UserIdentitySubmit>().ToTable("user_identity_submit");
        modelBuilder.Entity<Event>().ToTable("event");
        modelBuilder.Entity<EventEntry>().ToTable("event_entry");
    }
}

