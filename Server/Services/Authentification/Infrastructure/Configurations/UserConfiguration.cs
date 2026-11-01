using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Email)
            .IsRequired();
        builder.HasIndex(x => x.Email)
            .IsUnique()
           .HasFilter("\"IsDeleted\" = false");
           
        builder.Property(x => x.PasswordHash)
            .IsRequired();
        builder.Property(x => x.Role)
            .IsRequired()
            .HasConversion<string>();
        
        
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
