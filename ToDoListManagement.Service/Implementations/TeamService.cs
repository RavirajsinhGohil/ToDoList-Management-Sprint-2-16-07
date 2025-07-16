using ToDoListManagement.Entity.Helper;
using ToDoListManagement.Entity.Models;
using ToDoListManagement.Entity.ViewModel;
using ToDoListManagement.Repository.Interfaces;
using ToDoListManagement.Service.Interfaces;

namespace ToDoListManagement.Service.Implementations;

public class TeamService : ITeamService
{
    private readonly ITeamUserRepository _teamUserRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public TeamService(ITeamUserRepository teamUserRepository, IProjectRepository projectRepository, IEmployeeRepository employeeRepository)
    {
        _teamUserRepository = teamUserRepository;
        _projectRepository = projectRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<Pagination<EmployeeViewModel>> GetMembersAsync(Pagination<EmployeeViewModel> pagination, int teamManagerId)
    {
        Pagination<User> userPagination = new()
        {
            SearchKeyword = pagination.SearchKeyword,
            CurrentPage = pagination.CurrentPage,
            PageSize = pagination.PageSize,
            SortColumn = pagination.SortColumn,
            SortDirection = pagination.SortDirection
        };
        Pagination<TeamUserMapping> teamMembers = await _teamUserRepository.GetAllTeamMembersAsync(teamManagerId, userPagination);
        IOrderedEnumerable<TeamUserMapping>? sortedMembers = pagination.SortColumn switch
        {
            "reportingPerson" => (pagination.SortDirection?.ToLower() ?? "asc") == "asc"
                            ? teamMembers.Items.OrderBy(p => p.TeamManager?.Name)
                            : teamMembers.Items.OrderByDescending(p => p.TeamManager?.Name),

            "email" => (pagination.SortDirection?.ToLower() ?? "asc") == "asc"
                            ? teamMembers.Items.OrderBy(p => p.TeamMember?.Email)
                            : teamMembers.Items.OrderByDescending(p => p.TeamMember?.Email),

            "roleName" => (pagination.SortDirection?.ToLower() ?? "asc") == "asc"
                            ? teamMembers.Items.OrderBy(p => p.TeamMember?.Role?.RoleName)
                            : teamMembers.Items.OrderByDescending(p => p.TeamMember?.Role?.RoleName),

            _ => pagination.SortDirection?.ToLower() == "asc"
                            ? teamMembers.Items.OrderBy(p => p.TeamMember?.Name)
                            : teamMembers.Items.OrderByDescending(p => p.TeamMember?.Name)
        };
        List<EmployeeViewModel> members = [];
        foreach (TeamUserMapping employee in sortedMembers)
        {
            if(employee.TeamMember != null)
            {
                ProjectUser? projectUser = await _projectRepository.GetProjectByUserIdAsnc(employee.TeamMember.UserId);
                ProjectViewModel projectViewModel = new();

                if (projectUser != null)
                {
                    Project? project = projectUser.Project;
                    if (project != null)
                    {
                        projectViewModel.ProjectId = project.ProjectId;
                        projectViewModel.ProjectName = project.ProjectName;
                    }
                }

                members.Add(new EmployeeViewModel
                {
                    EmployeeId = employee.UserId,
                    Name = employee.TeamMember.Name ?? string.Empty,
                    Email = employee.TeamMember.Email ?? string.Empty,
                    PhoneNumber = employee.TeamMember.PhoneNumber ?? string.Empty,
                    Role = employee.TeamMember.RoleId,
                    RoleName = employee.TeamMember.Role?.RoleName ?? string.Empty,
                    Status = employee.TeamMember.IsActive ? "Active" : "Inactive",
                    Project = projectViewModel,
                    ReportingPerson = employee.TeamManager != null ? new UserViewModel() {
                        UserId = employee.TeamManager.UserId,
                        Name = employee.TeamManager.Name,
                        Email = employee.TeamManager.Email,
                        Role = employee.TeamManager.Role?.RoleName
                    } : null
                });
            }
        }

        return new Pagination<EmployeeViewModel>
        {
            Items = members,
            CurrentPage = teamMembers.CurrentPage,
            TotalPages = teamMembers.TotalPages,
            TotalRecords = teamMembers.TotalRecords,
            PageSize = teamMembers.PageSize
        };
    }

    public async Task<List<EmployeeViewModel>?> GetNotAssignedMembersAsync()
    {

        List<User> notAssignedMembers = await _employeeRepository.GetNotAssignedUsersAsync();
        List<EmployeeViewModel> members = [];
        foreach (User member in notAssignedMembers)
        {
            members.Add(new EmployeeViewModel()
            {
                EmployeeId = member.UserId,
                Name = member.Name,
                Email = member.Email,
                PhoneNumber = member.PhoneNumber
            });
        }

        return members;
    }

    public async Task<List<UserViewModel>> GetAllTeamMemberNamesAsync(int teamManagerid)
    {
        List<TeamUserMapping> teamMembers = await _teamUserRepository.GetAllTeamMemberNamesAsync(teamManagerid);
        List<UserViewModel> teamMembersViews = [];
        foreach(TeamUserMapping? teamMember in teamMembers)
        {
            teamMembersViews.Add(new UserViewModel() {
                UserId = teamMember.UserId ?? 0,
                Name = teamMember.TeamMember?.Name
            });
        }
        return teamMembersViews;
    }
}