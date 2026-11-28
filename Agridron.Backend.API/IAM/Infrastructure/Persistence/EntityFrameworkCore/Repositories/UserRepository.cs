using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Domain.Model.ValueObjects;
using Agridron.Backend.API.IAM.Domain.Repositories;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.IAM.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

internal sealed class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        EmailAddress normalizedEmail;
        try
        {
            normalizedEmail = EmailAddress.Create(email);
        }
        catch (DomainException)
        {
            return null;
        }

        return await Context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        EmailAddress normalizedEmail;
        try
        {
            normalizedEmail = EmailAddress.Create(email);
        }
        catch (DomainException)
        {
            return false;
        }

        return await Context.Set<User>()
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<User?> FindByVerificationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var normalizedToken = token.Trim();
        return await Context.Set<User>()
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == normalizedToken, cancellationToken);
    }
}
