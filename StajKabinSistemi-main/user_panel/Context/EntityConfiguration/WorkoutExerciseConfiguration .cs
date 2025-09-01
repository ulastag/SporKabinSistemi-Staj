using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using user_panel.Entity;

public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise> {
    public void Configure(EntityTypeBuilder<WorkoutExercise> builder) {
        builder.HasKey(we => new { we.WorkoutPlanId, we.ExerciseID });

        builder.HasOne(we => we.WorkoutPlan)
               .WithMany(w => w.WorkoutExercises)
               .HasForeignKey(we => we.WorkoutPlanId);

        builder.HasOne(we => we.Exercise)
               .WithMany(e => e.WorkoutExercises)
               .HasForeignKey(we => we.ExerciseID);
    }
}
