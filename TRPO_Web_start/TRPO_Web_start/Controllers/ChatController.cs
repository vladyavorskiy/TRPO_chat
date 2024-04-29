//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;

//namespace TRPO_Web_start.Controllers;

//public class ChatController : ControllerBase
//{
//    private readonly IHubContext<MainHub> _hubContext;
//    private readonly IHttpContextAccessor _httpContextAccessor;

//    public ChatController(IHubContext<MainHub> hubContext, IHttpContextAccessor httpContextAccessor)
//    {
//        _hubContext = hubContext;
//        _httpContextAccessor = httpContextAccessor;
//    }

//    [HttpPost("create")]
//    public async Task<IActionResult> CreateChat(string groupName)
//    {
//        await _hubContext.Clients.Group(groupName).SendAsync("onMessage", "Chat created!");
//        return Ok();
//    }

//    [HttpPost("join")]
//    public async Task<IActionResult> JoinChat(string groupName)
//    {
//        var connectionId = _httpContextAccessor.HttpContext.Connection.Id;
//        await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
//        return Ok();
//    }

//    [HttpPost("send")]
//    public async Task<IActionResult> SendMessageToChat(string groupName, string message)
//    {
//        await _hubContext.Clients.Group(groupName).SendAsync("onMessage", message);
//        return Ok();
//    }
//}