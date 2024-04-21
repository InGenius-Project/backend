namespace IngBackendApi;

using IngBackendApi.Controllers;
using IngBackendApi.Interfaces.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoWrapper.Wrappers;
using IngBackendApi.Models.DTO;
using IngBackendApi.Models.DBEntity;
using IngBackendApi.Application.Hubs;

[ApiController]
[Route("api/[controller]")]
public class ChatController(IHubContext<ChatHub> hubContext, IUserService userService) : BaseController
{
    private readonly IHubContext<ChatHub> _hubContext = hubContext;
    private readonly IUserService _userService = userService;





}
