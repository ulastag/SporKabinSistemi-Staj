using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using user_panel.Data;
using user_panel.Entity;

namespace user_panel.Context.EntityConfiguration {
    public class UserWorkoutConfiguration: IEntityTypeConfiguration<UserWorkout> {
       

        public void Configure(EntityTypeBuilder<UserWorkout> builder) {
            builder.HasKey(uw => new { uw.UserId, uw.WorkoutPlanId });
            builder.Property(uw => uw.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");
            builder.HasOne(uw => uw.User)
                   .WithMany(u => u.UserWorkouts)
                   .HasForeignKey(uw => uw.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(uw => uw.WorkoutPlan)
                   .WithMany(w => w.UserWorkouts)
                   .HasForeignKey(uw => uw.WorkoutPlanId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.Property(uw => uw.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);
        }
    }
}
