using ItMarathon.Dal.Common;
using ItMarathon.Dal.Common.Contracts;
using ItMarathon.Dal.Context;
using ItMarathon.Dal.Entities;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace ItMarathon.Dal.Repositories;

public class ProposalRepository(ApplicationDbContext repositoryContext) :
    RepositoryBase<Proposal>(repositoryContext), IProposalRepository
{
    public async Task<DataPage<Proposal>> GetProposalsAsync(
        bool trackChanges,
        ODataQueryOptions queryOptions,
        int? top,
        int? skip)
    {
        IQueryable<Proposal> query = FindAll(trackChanges);

        if (queryOptions != null)   
        {
            query = (IQueryable<Proposal>)queryOptions.ApplyTo(query);
        }

        var totalProposalsCount = await query.CountAsync();
        
        if (queryOptions?.OrderBy == null)
        {
            query = query.OrderBy(p => p.Id);
        }

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }
        
        if (top.HasValue)
        {
            query = query.Take(top.Value);
        }

        query = query
            .Include(p => p.AppUser)
            .Include(p => p.Photos)
            .Include(p => p.Properties!)
                .ThenInclude(properties => properties.PropertyDefinition)
            .Include(p => p.Properties!)
                .ThenInclude(properties => properties.PredefinedValue)
                    .ThenInclude(prop => prop!.ParentPropertyValue);

        var proposals = await query.ToListAsync();
        
        return new DataPage<Proposal>(proposals, totalProposalsCount);
    }

    public async Task<Proposal?> GetProposalAsync(long proposalId, bool trackChanges)
        => await FindByCondition(c => c.Id.Equals(proposalId), trackChanges)
        .Include(p => p.AppUser)
        .Include(p => p.Photos)
        .Include(p => p.Properties!)
            .ThenInclude(properties => properties.PropertyDefinition)
        .Include(p => p.Properties!)
            .ThenInclude(properties => properties.PredefinedValue)
                .ThenInclude(prop => prop!.ParentPropertyValue)
        .SingleOrDefaultAsync();

    public void CreateProposal(Proposal proposal) => Create(proposal);

    public void DeleteProposal(Proposal proposal) => Delete(proposal);
}
