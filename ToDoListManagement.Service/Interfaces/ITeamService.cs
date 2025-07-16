using ToDoListManagement.Entity.Helper;
using ToDoListManagement.Entity.ViewModel;

namespace ToDoListManagement.Service.Interfaces;

public interface ITeamService
{
    Task<Pagination<EmployeeViewModel>> GetMembersAsync(Pagination<EmployeeViewModel> pagination, int teamManagerId);
    Task<List<EmployeeViewModel>?> GetNotAssignedMembersAsync();
    Task<List<UserViewModel>> GetAllTeamMemberNamesAsync(int teamManagerid);
}
