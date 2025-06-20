﻿using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Base.Interface;

namespace Infrastructure.Data.Postgres.Repositories.Interface;

public interface IOrganizationRepository : ITrackedEntityRepository<Organization, int>
{
    public Task<int> SoftDelete(Organization organization);
    public Task<(IList<Organization> Items, int TotalCount)> GetPagedOrganizationsAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        bool includeDeleted = false,
        bool tracked = false);
}
