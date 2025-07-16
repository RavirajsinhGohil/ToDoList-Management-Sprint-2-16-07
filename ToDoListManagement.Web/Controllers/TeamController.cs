using Microsoft.AspNetCore.Mvc;
using ToDoListManagement.Entity.Constants;
using ToDoListManagement.Entity.Helper;
using ToDoListManagement.Entity.ViewModel;
using ToDoListManagement.Service.Helper;
using ToDoListManagement.Service.Interfaces;

namespace ToDoListManagement.Web.Controllers;

public class TeamController : BaseController
{
    private readonly ITeamService _teamService;
    public TeamController(IAuthService authService, ITeamService teamService) : base(authService)
    {
        _teamService = teamService;
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetAssignedMembers(int draw, int start, int length, string searchValue, string sortColumn, string sortDirection)
    {
        int pageNumber = start / length + 1;
        int pageSize = length;

        Pagination<EmployeeViewModel>? pagination = new()
        {
            SearchKeyword = searchValue,
            CurrentPage = pageNumber,
            PageSize = pageSize,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };

        if (SessionUser == null)
        {
            return Json(new
            {
                draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = Array.Empty<EmployeeViewModel>()
            });
        }
        Pagination<EmployeeViewModel>? data = await _teamService.GetMembersAsync(pagination, SessionUser.UserId);
        return Json(new
        {
            draw,
            recordsTotal = data.TotalRecords,
            recordsFiltered = data.TotalRecords,
            data = data.Items
        });
    }

    public async Task<IActionResult> GetNotAssignedMembers()
    {
        List<EmployeeViewModel>? data = await _teamService.GetNotAssignedMembersAsync();
        return Json(data);
    }

    [CustomAuthorize([Constants.ManageTeamModule], Constants.CanAddEdit)]
    [HttpGet]
    public async Task<IActionResult> GetTeamMembersJson()
    {
        if (SessionUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        List<UserViewModel> model = await _teamService.GetAllTeamMemberNamesAsync(SessionUser.UserId);
        return Json(model);
    }
}