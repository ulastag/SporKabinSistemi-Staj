using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using user_panel.Entity;

public class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan> {
    public void Configure(EntityTypeBuilder<WorkoutPlan> builder) {
        builder.HasKey(w => w.WorkoutPlanId);

        builder.Property(w => w.WorkoutName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(w => w.WorkoutPreview)
               .HasMaxLength(255);

        builder.Property(w => w.WorkoutDetails)
               .HasColumnType("nvarchar(max)");
        
        builder.Property(e => e.WorkoutImage)
                .IsRequired(false);

    }

}