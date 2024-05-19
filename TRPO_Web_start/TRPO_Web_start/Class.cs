using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TRPO_Web_start
{
    public class MainHub : Hub
    {
        private readonly IChatStateService _chatStateService;

        public MainHub(IChatStateService chatStateService)
        {
            _chatStateService = chatStateService;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userName = Uri.UnescapeDataString(httpContext.Request.Query["access_token"]);

            if (!string.IsNullOrEmpty(userName))
            {
                if (_chatStateService.UserConnections.ContainsKey(userName))
                {
                    _chatStateService.UserConnections[userName] = Context.ConnectionId;
                }
                else
                {
                    _chatStateService.UserConnections.Add(userName, Context.ConnectionId);
                }

                if (_chatStateService.UserGroups.ContainsKey(userName))
                {
                    foreach (var group in _chatStateService.UserGroups[userName])
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, group);
                    }
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = _chatStateService.UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (userName != null)
            {
                _chatStateService.UserConnections.Remove(userName);
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}