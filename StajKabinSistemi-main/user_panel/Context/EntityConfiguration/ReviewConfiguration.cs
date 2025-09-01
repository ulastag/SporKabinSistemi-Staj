using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using user_panel.Entity;

namespace user_panel.Context.EntityConfiguration
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Content).IsRequired();
            builder.Property(r => r.Rating).IsRequired();

            // Bir kullanıcının bir kabine sadece bir yorum yapabilmesini sağlayan
            // benzersiz bir kısıtlama (unique constraint) ekliyoruz.
            // Bu, veri bütünlüğü için çok önemlidir.
            builder.HasIndex(r => new { r.ApplicationUserId, r.CabinId }).IsUnique();

            // Review -> ApplicationUser ilişkisi (Bir kullanıcı çok yorum yapabilir)
            builder.HasOne(r => r.ApplicationUser)
                   .WithMany(u => u.Reviews)
                   .HasForeignKey(r => r.ApplicationUserId)
                   .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirse yorumları kalsın.

            // Review -> Cabin ilişkisi (Bir kabinin çok yorumu olabilir)
            builder.HasOne(r => r.Cabin)
                   .WithMany(c => c.Reviews)
                   .HasForeignKey(r => r.CabinId)
                   .OnDelete(DeleteBehavior.Cascade); // Kabin silinirse yorumları da silinsin.
        }
    }
}