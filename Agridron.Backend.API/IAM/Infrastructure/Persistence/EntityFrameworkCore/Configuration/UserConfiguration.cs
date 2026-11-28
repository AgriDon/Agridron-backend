using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Agridron.Backend.API.IAM.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    private static readonly ValueConverter<UserId, long> UserIdConverter =
        new(v => v.Value, v => new UserId(v));

    private static readonly ValueConverter<EmailAddress, string> EmailAddressConverter =
        new(v => v.Value, v => new EmailAddress(v));

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasConversion(UserIdConverter)
            .ValueGeneratedOnAdd();

        builder.Property(u => u.FullName)
            .HasColumnName("full_name")
            .IsRequired()
            .HasMaxLength(User.MaxFullNameLength);

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasConversion(EmailAddressConverter)
            .IsRequired()
            .HasMaxLength(User.MaxEmailLength);

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(User.MaxPasswordHashLength);

        builder.Property(u => u.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(User.MaxPhoneNumberLength);

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(u => u.EmailVerificationToken)
            .HasColumnName("email_verification_token")
            .HasMaxLength(User.MaxVerificationTokenLength);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ux_users_email");

        builder.HasIndex(u => u.EmailVerificationToken)
            .HasDatabaseName("ix_users_email_verification_token");

        builder.HasIndex(u => u.Role)
            .HasDatabaseName("ix_users_role");

        builder.HasIndex(u => u.Status)
            .HasDatabaseName("ix_users_status");
    }
}
