﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace IngBackend.Controllers;


[Authorize]
public class BaseController : Controller
{
    public Guid? UserId
    {
        get
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            return null;

        }
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewData["UserId"] = UserId;
        base.OnActionExecuting(context);
    }
}
