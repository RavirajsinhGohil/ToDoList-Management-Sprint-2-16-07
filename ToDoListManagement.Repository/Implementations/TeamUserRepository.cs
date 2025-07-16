using Microsoft.EntityFrameworkCore;
using ToDoListManagement.Entity.Data;
using ToDoListManagement.Entity.Helper;
using ToDoListManagement.Entity.Models;
using ToDoListManagement.Repository.Interfaces;

namespace ToDoListManagement.Repository.Implementations;

public class TeamUserRepository : ITeamUserRepository
{
    private readonly ToDoListDbContext _context;
    public TeamUserRepository(ToDoListDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeamUserMapping>> GetAllAsync()
    {
        return await _context.Set<TeamUserMapping>().ToListAsync();
    }

    public async Task<TeamUserMapping?> GetByIdAsync(int id)
    {
        return await _context.Set<TeamUserMapping>().FindAsync(id);
    }

    public async Task<bool> AddAsync(TeamUserMapping entity)
    {
        await _context.Set<TeamUserMapping>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(TeamUserMapping entity)
    {
        _context.Set<TeamUserMapping>().Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Pagination<TeamUserMapping>> GetAllTeamMembersAsync(int teamManagerId, Pagination<User> pagination)
    {
        IQueryable<TeamUserMapping>? query = _context.TeamUserMappings
                                    .Where(t => t.TeamManagerId == teamManagerId && !t.IsDeleted)
                                    .Include(t => t.TeamManager)
                                    .ThenInclude(tm => tm.Role)
                                    .Include(t => t.TeamMember)
                                    .ThenInclude(tm => tm.Role)
                                    .Where(u => !u.IsDeleted);

        if (!string.IsNullOrEmpty(pagination.SearchKeyword))
        {
            query = query.Where(u =>
                (!string.IsNullOrEmpty(u.TeamMember.Name) && u.TeamMember.Name.ToLower().Contains(pagination.SearchKeyword.Trim().ToLower())) ||
                (!string.IsNullOrEmpty(u.TeamMember.Email) && u.TeamMember.Email.ToLower().Contains(pagination.SearchKeyword.Trim().ToLower()))
            );
        }

        query = pagination.SortColumn switch
        {
            "reportingPerson" => (pagination.SortDirection?.ToLower() ?? "asc") == "asc"
                            ? query.OrderBy(p => p.TeamManager.Name)
                            : query.OrderByDescending(p => p.TeamManager.Name),

            "email" => (pagination.SortDirection?.ToLower() ?? "asc") == "asc"
                            ? query.OrderBy(p => p.TeamMember.Email)
                            : query.OrderByDescending(p => p.TeamMember.Email),

            "role" => (pagination.SortDirection?.ToLower() ?? "asc") == "asc"
                            ? query.OrderBy(p => p.TeamMember.Role.RoleName)
                            : query.OrderByDescending(p => p.TeamMember.Role.RoleName),

            _ => pagination.SortDirection?.ToLower() == "asc"
                            ? query.OrderBy(p => p.TeamMember.Name)
                            : query.OrderByDescending(p => p.TeamMember.Name)
        };

        List<TeamUserMapping> pagedData = await query
            .Skip((pagination.CurrentPage - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        HashSet<TeamUserMapping> allMembers = new();
        foreach (TeamUserMapping user in pagedData)
        {
            allMembers.Add(user);
            await GetSubTeamMembers(user.UserId ?? 0, allMembers, pagination);
        }

        int totalRecords = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

        return new Pagination<TeamUserMapping>
        {
            Items = allMembers.ToList(),
            CurrentPage = pagination.CurrentPage,
            TotalPages = totalPages,
            TotalRecords = totalRecords,
            PageSize = pagination.PageSize
        };
    }

    private async Task GetSubTeamMembers(int teamManagerId, HashSet<TeamUserMapping> members, Pagination<User> pagination)
    {
        IQueryable<TeamUserMapping> query = _context.TeamUserMappings
                                    .Where(t => t.TeamManagerId == teamManagerId && !t.IsDeleted)
                                    .Include(t => t.TeamManager)
                                    .ThenInclude(tm => tm.Role)
                                    .Include(t => t.TeamMember)
                                    .ThenInclude(tm => tm.Role)
                                    .Where(u => !u.IsDeleted);

        if (!string.IsNullOrEmpty(pagination.SearchKeyword))
        {
            query = query.Where(u =>
                (!string.IsNullOrEmpty(u.TeamMember.Name) && u.TeamMember.Name.ToLower().Contains(pagination.SearchKeyword.Trim().ToLower())) ||
                (!string.IsNullOrEmpty(u.TeamMember.Email) && u.TeamMember.Email.ToLower().Contains(pagination.SearchKeyword.Trim().ToLower()))
            );
        }

        query = pagination.SortColumn switch
        {
            "reportingPerson" => (pagination.SortDirection?.ToLower() ?? "asc") == "asc"
                            ? query.OrderBy(p => p.TeamManager.Name)
                            : query.OrderByDescending(p => p.TeamManager.Name),

            "email" => (pagination.SortDirection?.ToLower() ?? "asc") == "asc"
                            ? query.OrderBy(p => p.TeamMember.Email)
                            : query.OrderByDescending(p => p.TeamMember.Email),

            "role" => (pagination.SortDirection?.ToLower() ?? "asc") == "asc"
                            ? query.OrderBy(p => p.TeamMember.Role.RoleName)
                            : query.OrderByDescending(p => p.TeamMember.Role.RoleName),

            _ => pagination.SortDirection?.ToLower() == "asc"
                            ? query.OrderBy(p => p.TeamMember.Name)
                            : query.OrderByDescending(p => p.TeamMember.Name)
        };

        List<TeamUserMapping> subTeamMembers = await query
            .Skip((pagination.CurrentPage - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        foreach (TeamUserMapping subTeamMember in subTeamMembers)
        {
            if (!members.Contains(subTeamMember))
            {
                members.Add(subTeamMember);
                await GetSubTeamMembers(subTeamMember.UserId ?? 0, members, pagination);
            }
        }
    }

    public async Task<List<TeamUserMapping>> GetAllTeamMemberNamesAsync(int teamManagerId)
    {
        return await _context.TeamUserMappings
                        .Where(t => t.TeamManagerId == teamManagerId && !t.IsDeleted)
                        .Include(t => t.TeamMember)
                        .Where(u => !u.IsDeleted).ToListAsync();
    }
}