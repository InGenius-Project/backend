using IngBackend.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngBackend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TESTController: BaseController
{
    public TESTController() { }

    [HttpGet]
    public string GetGAPassword()
    {
        return Helper.GetSAPassword();
    }
}
