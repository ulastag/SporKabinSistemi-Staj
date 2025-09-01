using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using user_panel.Entity;

namespace user_panel.Context.EntityConfiguration {
    public class HowToDoConfiguration : IEntityTypeConfiguration<HowToDo> {
        public void Configure(EntityTypeBuilder<HowToDo> builder) {
            builder.HasKey(h => h.HowToDoId);

            builder.Property(h => h.HowToDoStep)
                   .IsRequired()
                   .HasColumnType("nvarchar(max)");
        }
    }
}
