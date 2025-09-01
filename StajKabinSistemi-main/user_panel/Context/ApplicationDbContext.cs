using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using user_panel.Context.EntityConfiguration;
using user_panel.Context.Helpers;
using user_panel.Data;
using user_panel.Entity;

namespace user_panel.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cabin> Cabins { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<user_panel.Models.ImageModel> Images { get; set; }
        public DbSet<LogEntry> Logs { get; set; }
        public DbSet<Exercise> Exercises { get; set; }   
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; }
        public DbSet<HowToDo> HowToDos { get; set; }
        public DbSet<WorkoutExercise> WorkoutExercises { get; set; }    
        public DbSet<UserWorkout> UserWorkouts { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            builder.Seed();
        }
    }
}