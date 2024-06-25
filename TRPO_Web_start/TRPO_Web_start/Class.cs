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
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = _chatStateService.UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (userName != null)
            {
                foreach (var group in _chatStateService.UserGroups[Context.ConnectionId])
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

    }
}