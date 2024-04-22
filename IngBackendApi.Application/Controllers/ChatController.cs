namespace IngBackendApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class ChatController() : BaseController { }
