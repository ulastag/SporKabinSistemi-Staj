using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using user_panel.Entity;

namespace user_panel.Context.EntityConfiguration {
    public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise> {
        public void Configure(EntityTypeBuilder<Exercise> builder) {
            builder.HasKey(e => e.ExerciseID);

            builder.Property(e => e.ExerciseName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.ExerciseDetails)
                   .HasColumnType("nvarchar(max)");

            builder.HasMany(e => e.HowToDos)
                   .WithOne()
                   .HasForeignKey(h => h.ExerciseId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
