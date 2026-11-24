using Entities;
using Microsoft.EntityFrameworkCore;

namespace EfcRepositories;

public class AppContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<ProfileModule> ProfileModules => Set<ProfileModule>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    // used by the "dotnet ef" tools, run from the EfcRepositories folder
    public AppContext()
    {
    }

    // used by the Web API, which passes in the path to the database file
    public AppContext(DbContextOptions<AppContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=app.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Deleting something removes everything that belongs to it
        // (a user's posts, a post's comments, a comment's replies, ...).

        modelBuilder.Entity<User>(user =>
        {
            user.HasIndex(u => u.UserName).IsUnique();
            user.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Follow>(follow =>
        {
            follow.HasIndex(f => new { f.FollowerId, f.FollowingId }).IsUnique();
            follow.HasOne(f => f.Follower).WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId).OnDelete(DeleteBehavior.Cascade);
            follow.HasOne(f => f.Following).WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Post>(post =>
        {
            post.HasOne(p => p.User).WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PostLike>(like =>
        {
            like.HasIndex(l => new { l.PostId, l.UserId }).IsUnique();
            like.HasOne(l => l.Post).WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId).OnDelete(DeleteBehavior.Cascade);
            like.HasOne(l => l.User).WithMany(u => u.PostLikes)
                .HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PostTag>(postTag =>
        {
            postTag.HasIndex(pt => new { pt.PostId, pt.TagId }).IsUnique();
            postTag.HasOne(pt => pt.Post).WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostId).OnDelete(DeleteBehavior.Cascade);
            postTag.HasOne(pt => pt.Tag).WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PostMedia>(media =>
        {
            media.HasOne(m => m.Post).WithMany(p => p.Media)
                .HasForeignKey(m => m.PostId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(comment =>
        {
            comment.HasOne(c => c.Post).WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade);
            comment.HasOne(c => c.User).WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            comment.HasOne(c => c.ParentComment).WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommentLike>(like =>
        {
            like.HasIndex(l => new { l.CommentId, l.UserId }).IsUnique();
            like.HasOne(l => l.Comment).WithMany(c => c.Likes)
                .HasForeignKey(l => l.CommentId).OnDelete(DeleteBehavior.Cascade);
            like.HasOne(l => l.User).WithMany(u => u.CommentLikes)
                .HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Tag>(tag =>
        {
            tag.HasIndex(t => t.Name).IsUnique();
        });

        modelBuilder.Entity<Profile>(profile =>
        {
            // one profile per user
            profile.HasIndex(p => p.UserId).IsUnique();
            profile.HasOne(p => p.User).WithOne(u => u.Profile)
                .HasForeignKey<Profile>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfileModule>(module =>
        {
            module.HasOne(m => m.User).WithMany(u => u.ProfileModules)
                .HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(notification =>
        {
            notification.HasIndex(n => new { n.UserId, n.CreatedDate });
            notification.HasOne(n => n.User).WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
            notification.HasOne(n => n.Actor).WithMany(u => u.CausedNotifications)
                .HasForeignKey(n => n.ActorId).OnDelete(DeleteBehavior.Cascade);
            notification.HasOne(n => n.Post).WithMany(p => p.Notifications)
                .HasForeignKey(n => n.PostId).OnDelete(DeleteBehavior.Cascade);
            notification.HasOne(n => n.Comment).WithMany(c => c.Notifications)
                .HasForeignKey(n => n.CommentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Conversation>(conversation =>
        {
            conversation.HasOne(c => c.UserOne).WithMany(u => u.ConversationsAsUserOne)
                .HasForeignKey(c => c.UserOneId).OnDelete(DeleteBehavior.Cascade);
            conversation.HasOne(c => c.UserTwo).WithMany(u => u.ConversationsAsUserTwo)
                .HasForeignKey(c => c.UserTwoId).OnDelete(DeleteBehavior.Cascade);
            conversation.HasOne(c => c.StartedBy).WithMany(u => u.StartedConversations)
                .HasForeignKey(c => c.StartedById).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(message =>
        {
            message.HasOne(m => m.Conversation).WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId).OnDelete(DeleteBehavior.Cascade);
            message.HasOne(m => m.Sender).WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
