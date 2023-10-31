using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserLike>(entity => {
            // The entity UserLike has a primary key made of the SourceUserId and TargetUserId.
            // This is going to represent the primary that is used in the Likes table.
            entity.HasKey(k => new {k.SourceUserId, k.TargetUserId});

            // Configure the relationships.
            // One user can like many users.
            entity.HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Once user can be liked by many users.
            entity.HasOne(s => s.TargetUser)
                .WithMany(l => l.LikedByUsers)
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Code from Udemy: Chapter 174: Adding a likes entity.
        // builder.Entity<UserLike>().HasKey(k => new {k.SourceUserId, k.TargetUserId});
        
        // builder.Entity<UserLike>()
        //     .HasOne(s => s.SourceUser)
        //     .WithMany(l => l.LikedUsers)
        //     .HasForeignKey(s => s.SourceUserId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // builder.Entity<UserLike>()
        //     .HasOne(s => s.TargetUser)
        //     .WithMany(l => l.LikedByUsers)
        //     .HasForeignKey(s => s.TargetUserId)
        //     .OnDelete(DeleteBehavior.Cascade);
    }
}
