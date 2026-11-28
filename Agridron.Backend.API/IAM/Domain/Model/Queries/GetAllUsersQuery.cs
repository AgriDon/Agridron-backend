using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.IAM.Domain.Model.Queries;

public sealed record GetAllUsersQuery(PaginationParameters Pagination);
