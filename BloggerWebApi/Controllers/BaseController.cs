using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloggerWebApi.Controllers
{
    public class BaseController : ControllerBase
    {
        protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        protected bool IsAdmin() => User.IsInRole("Admin");

        protected bool IsCurrentUser(string id) => CurrentUserId == id;

        protected bool IsAdminOrCurrentUser(string id) => IsAdmin() || IsCurrentUser(id);
    }

}
