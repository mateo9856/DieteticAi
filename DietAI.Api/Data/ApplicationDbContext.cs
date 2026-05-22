using Microsoft.EntityFrameworkCore;
using DietAI.Api.Services.AiPlanSender.Models;

namespace DietAI.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Diets> Diets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Diets>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Id);
            entity.Property(e => e.DietName).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.GoalType).IsRequired();
            entity.Property(e => e.ActivityLevel).IsRequired();
            entity.Property(e => e.MealsPerDay).IsRequired();
            entity.Property(e => e.Allergies).IsRequired();
            entity.Property(e => e.ExcludedIngredients).IsRequired();
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            entity.HasIndex(e => new { e.UserId, e.CreatedAtUtc });
        });
    }
}
