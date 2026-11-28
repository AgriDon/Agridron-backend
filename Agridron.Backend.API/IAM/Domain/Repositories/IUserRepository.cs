using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.IAM.Domain.Repositories;

/// <summary>Repository contract for <see cref="User" /> aggregates.</summary>
public interface IUserRepository : IBaseRepository<User>
{
    /// <summary>Finds a user by normalized email address.</summary>
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Checks whether an account with the given email address already exists.</summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Finds a user by its pending email verification token.</summary>
    Task<User?> FindByVerificationTokenAsync(string token, CancellationToken cancellationToken = default);
}
