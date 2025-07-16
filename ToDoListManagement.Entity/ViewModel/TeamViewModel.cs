namespace ToDoListManagement.Entity.ViewModel;

public class TeamViewModel
{
    public EmployeeViewModel TeamManager { get; set; } = new();

    public List<EmployeeViewModel> TeamMembers { get; set; } = [];
    
}