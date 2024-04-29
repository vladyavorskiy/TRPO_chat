using Microsoft.AspNetCore.SignalR;

namespace TRPO_Web_start
{
    //public class MainHub : Hub
    //{
    //    private static readonly Dictionary<string, string> ConnectionsGroup = new Dictionary<string, string>();

    //    public override async Task OnConnectedAsync()
    //    {
    //        await base.OnConnectedAsync();
    //    }

    //    public override async Task OnDisconnectedAsync(Exception exception)
    //    {
    //        if (ConnectionsGroup.ContainsKey(Context.ConnectionId))
    //        {
    //            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConnectionsGroup[Context.ConnectionId]);
    //            ConnectionsGroup.Remove(Context.ConnectionId);
    //        }
    //        await base.OnDisconnectedAsync(exception);
    //    }

    //    public async Task JoinGroup(string group)
    //    {
    //        if (ConnectionsGroup.ContainsKey(Context.ConnectionId))
    //        {
    //            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConnectionsGroup[Context.ConnectionId]);
    //            ConnectionsGroup.Remove(Context.ConnectionId);
    //        }
    //        ConnectionsGroup.Add(Context.ConnectionId, group);
    //        await Groups.AddToGroupAsync(Context.ConnectionId, group);
    //    }

    //    public async Task SendMessage(string roomName, string message)
    //    {
    //        await Clients.Group(roomName)
    //            .SendAsync("onMessage", message);
    //    }
    //}


    //public class MainHub : Hub
    //{
    //    private static readonly Dictionary<string, string> ConnectionsGroup = new Dictionary<string, string>();
    //    private static readonly Dictionary<string, List<string>> GroupMessages = new Dictionary<string, List<string>>();

    //    public override async Task OnConnectedAsync()
    //    {
    //        await base.OnConnectedAsync();
    //    }

    //    public override async Task OnDisconnectedAsync(Exception exception)
    //    {
    //        if (ConnectionsGroup.ContainsKey(Context.ConnectionId))
    //        {
    //            var group = ConnectionsGroup[Context.ConnectionId];
    //            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
    //            ConnectionsGroup.Remove(Context.ConnectionId);
    //        }
    //        await base.OnDisconnectedAsync(exception);
    //    }

    //    public async Task JoinGroup(string group)
    //    {
    //        if (ConnectionsGroup.ContainsKey(Context.ConnectionId))
    //        {
    //            var prevGroup = ConnectionsGroup[Context.ConnectionId];
    //            await Groups.RemoveFromGroupAsync(Context.ConnectionId, prevGroup);
    //            ConnectionsGroup[Context.ConnectionId] = group;
    //        }
    //        else
    //        {
    //            ConnectionsGroup.Add(Context.ConnectionId, group);
    //        }

    //        if (!GroupMessages.ContainsKey(group))
    //        {
    //            GroupMessages.Add(group, new List<string>());
    //        }

    //        // Send previous messages to the client
    //        foreach (var message in GroupMessages[group])
    //        {
    //            await Clients.Client(Context.ConnectionId).SendAsync("onMessage", message);
    //        }

    //        await Groups.AddToGroupAsync(Context.ConnectionId, group);
    //    }

    //    public async Task SendMessage(string groupName, string message)
    //    {
    //        if (!GroupMessages.ContainsKey(groupName))
    //        {
    //            GroupMessages.Add(groupName, new List<string>());
    //        }

    //        GroupMessages[groupName].Add(message);

    //        await Clients.Group(groupName).SendAsync("onMessage", message);
    //    }

    //    public async Task LeaveGroup(string group)
    //    {
    //        if (ConnectionsGroup.ContainsKey(Context.ConnectionId))
    //        {
    //            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
    //            ConnectionsGroup.Remove(Context.ConnectionId);
    //        }
    //    }
    //}



    //public class MainHub : Hub
    //{
    //    private static readonly Dictionary<string, List<string>> UserGroups = new Dictionary<string, List<string>>();
    //    private static readonly Dictionary<string, List<string>> GroupMessages = new Dictionary<string, List<string>>();

    //    public override async Task OnConnectedAsync()
    //    {
    //        await base.OnConnectedAsync();
    //    }

    //    public override async Task OnDisconnectedAsync(Exception exception)
    //    {
    //        if (UserGroups.ContainsKey(Context.ConnectionId))
    //        {
    //            var userGroups = UserGroups[Context.ConnectionId];
    //            foreach (var group in userGroups)
    //            {
    //                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
    //            }
    //            UserGroups.Remove(Context.ConnectionId);
    //        }
    //        await base.OnDisconnectedAsync(exception);
    //    }

    //    public async Task JoinGroup(string group)
    //    {
    //        if (!UserGroups.ContainsKey(Context.ConnectionId))
    //        {
    //            UserGroups.Add(Context.ConnectionId, new List<string>());
    //        }

    //        if (!GroupMessages.ContainsKey(group))
    //        {
    //            // Group doesn't exist, send error message
    //            await Clients.Client(Context.ConnectionId).SendAsync("onMessage", $"Error: Group '{group}' does not exist.");
    //            return;
    //        }

    //        UserGroups[Context.ConnectionId].Add(group);

    //        // Send previous messages to the client
    //        foreach (var message in GroupMessages[group])
    //        {
    //            await Clients.Client(Context.ConnectionId).SendAsync("onMessage", message);
    //        }

    //        await Groups.AddToGroupAsync(Context.ConnectionId, group);
    //    }

    //    public async Task SendMessage(string groupName, string message)
    //    {
    //        if (!GroupMessages.ContainsKey(groupName))
    //        {
    //            GroupMessages.Add(groupName, new List<string>());
    //        }

    //        GroupMessages[groupName].Add(message);

    //        await Clients.Group(groupName).SendAsync("onMessage", message);
    //    }

    //    public async Task LeaveGroup(string group)
    //    {
    //        if (UserGroups.ContainsKey(Context.ConnectionId))
    //        {
    //            if (UserGroups[Context.ConnectionId].Contains(group))
    //            {
    //                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
    //                UserGroups[Context.ConnectionId].Remove(group);
    //            }
    //        }
    //    }

    //    public async Task<List<string>> GetUserGroups()
    //    {
    //        if (UserGroups.ContainsKey(Context.ConnectionId))
    //        {
    //            return UserGroups[Context.ConnectionId];
    //        }
    //        return new List<string>();
    //    }

    //    public async Task CreateGroup(string groupName)
    //    {
    //        // Check if group already exists
    //        if (!GroupMessages.ContainsKey(groupName))
    //        {
    //            GroupMessages.Add(groupName, new List<string>());
    //        }
    //        else
    //        {
    //            await Clients.Client(Context.ConnectionId).SendAsync("onMessage", $"Error: Group '{groupName}' already exists.");
    //        }
    //    }

    //    public async Task AddToGroup(string groupName)
    //    {
    //        await JoinGroup(groupName);
    //    }
    //}


    //public class MainHub : Hub
    //{
    //    private static readonly Dictionary<string, List<string>> UserGroups = new Dictionary<string, List<string>>();
    //    private static readonly Dictionary<string, List<string>> GroupMessages = new Dictionary<string, List<string>>();

    //    public override async Task OnConnectedAsync()
    //    {
    //        await base.OnConnectedAsync();
    //    }

    //    public override async Task OnDisconnectedAsync(Exception exception)
    //    {
    //        if (UserGroups.ContainsKey(Context.ConnectionId))
    //        {
    //            var userGroups = UserGroups[Context.ConnectionId];
    //            foreach (var group in userGroups)
    //            {
    //                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
    //            }
    //            UserGroups.Remove(Context.ConnectionId);
    //        }
    //        await base.OnDisconnectedAsync(exception);
    //    }

    //    //public async Task JoinGroup(string group)
    //    //{
    //    //    if (!GroupExists(group))
    //    //    {
    //    //        await Clients.Caller.SendAsync("onMessage", $"Group '{group}' does not exist.");
    //    //        return;
    //    //    }

    //    //    if (!UserGroups.ContainsKey(Context.ConnectionId))
    //    //    {
    //    //        UserGroups.Add(Context.ConnectionId, new List<string>());
    //    //    }

    //    //    UserGroups[Context.ConnectionId].Add(group);

    //    //    if (!GroupMessages.ContainsKey(group))
    //    //    {
    //    //        GroupMessages.Add(group, new List<string>());
    //    //    }

    //    //    // Send previous messages to the client
    //    //    foreach (var message in GroupMessages[group])
    //    //    {
    //    //        await Clients.Client(Context.ConnectionId).SendAsync("onMessage", message);
    //    //    }

    //    //    await Groups.AddToGroupAsync(Context.ConnectionId, group);
    //    //}

    //    //public async Task SendMessage(string groupName, string message)
    //    //{
    //    //    if (!GroupMessages.ContainsKey(groupName))
    //    //    {
    //    //        GroupMessages.Add(groupName, new List<string>());
    //    //    }

    //    //    GroupMessages[groupName].Add(message);

    //    //    await Clients.OthersInGroup(groupName).SendAsync("onMessage", message);
    //    //}

    //    public async Task JoinGroup(string group)
    //    {
    //        if (!GroupExists(group))
    //        {
    //            await Clients.Caller.SendAsync("onMessage", $"Group '{group}' does not exist.");
    //            return;
    //        }

    //        if (!UserGroups.ContainsKey(Context.ConnectionId))
    //        {
    //            UserGroups.Add(Context.ConnectionId, new List<string>());
    //        }

    //        // Проверяем, не состоит ли пользователь уже в этой группе
    //        if (!UserGroups[Context.ConnectionId].Contains(group))
    //        {
    //            UserGroups[Context.ConnectionId].Add(group);

    //            if (!GroupMessages.ContainsKey(group))
    //            {
    //                GroupMessages.Add(group, new List<string>());
    //            }

    //            // Send previous messages from this group to the client
    //            foreach (var message in GroupMessages[group])
    //            {
    //                await Clients.Client(Context.ConnectionId).SendAsync("onMessage", message);
    //            }

    //            await Groups.AddToGroupAsync(Context.ConnectionId, group);
    //        }
    //    }

    //    public async Task SendMessage(string groupName, string message)
    //    {
    //        if (!GroupMessages.ContainsKey(groupName))
    //        {
    //            GroupMessages.Add(groupName, new List<string>());
    //        }

    //        GroupMessages[groupName].Add($"{groupName}: {message}");

    //        await Clients.OthersInGroup(groupName).SendAsync("onMessage", $"{groupName}: {message}");
    //    }


    //    public async Task LeaveGroup(string group)
    //    {
    //        if (UserGroups.ContainsKey(Context.ConnectionId))
    //        {
    //            if (UserGroups[Context.ConnectionId].Contains(group))
    //            {
    //                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
    //                //UserGroups[Context.ConnectionId].Remove(group);
    //            }
    //        }
    //    }

    //    public async Task<List<string>> GetUserGroups()
    //    {
    //        if (UserGroups.ContainsKey(Context.ConnectionId))
    //        {
    //            return UserGroups[Context.ConnectionId];
    //        }
    //        return new List<string>();
    //    }

    //    public async Task CreateGroup(string groupName)
    //    {
    //        if (!GroupExists(groupName))
    //        {
    //            GroupMessages.Add(groupName, new List<string>());
    //        }
    //    }

    //    public async Task AddToGroup(string groupName)
    //    {
    //        if (!GroupExists(groupName))
    //        {
    //            await Clients.Caller.SendAsync("onMessage", $"Group '{groupName}' does not exist.");
    //            return;
    //        }

    //        await JoinGroup(groupName);
    //    }

    //    private bool GroupExists(string groupName)
    //    {
    //        return GroupMessages.ContainsKey(groupName);
    //    }
    //}




    public class MainHub : Hub
    {
        private static readonly Dictionary<string, List<string>> UserGroups = new Dictionary<string, List<string>>();
        private static readonly Dictionary<string, List<string>> GroupMessages = new Dictionary<string, List<string>>();

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (UserGroups.ContainsKey(Context.ConnectionId))
            {
                var userGroups = UserGroups[Context.ConnectionId];
                foreach (var group in userGroups)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
                }
                UserGroups.Remove(Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<int> JoinGroup(string group)
        {
            if (!GroupExists(group))
            {
                await Clients.Caller.SendAsync("onMessage", $"Group '{group}' does not exist.");
                return 0;
            }

            if (!UserGroups.ContainsKey(Context.ConnectionId))
            {
                UserGroups.Add(Context.ConnectionId, new List<string>());
            }


            if (!UserGroups[Context.ConnectionId].Contains(group))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
                UserGroups[Context.ConnectionId].Add(group);
                foreach (var message in GroupMessages[group])
                {
                    await Clients.Caller.SendAsync("onMessage", message);
                }
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
                foreach (var message in GroupMessages[group])
                {
                    await Clients.Caller.SendAsync("onMessage", message);
                }
            }

            return 1;
        }


        public async Task SendMessage(string groupName, string message)
        {
            if (!GroupMessages.ContainsKey(groupName))
            {
                GroupMessages.Add(groupName, new List<string>());
            }

            GroupMessages[groupName].Add($"{message}");

            await Clients.OthersInGroup(groupName).SendAsync("onMessage", $"{message}");
        }

        public async Task LeaveGroup(string group)
        {
            if (UserGroups.ContainsKey(Context.ConnectionId))
            {
                if (UserGroups[Context.ConnectionId].Contains(group))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
                    //UserGroups[Context.ConnectionId].Remove(group);
                }
            }
        }

        public async Task<List<string>> GetUserGroups()
        {
            if (UserGroups.ContainsKey(Context.ConnectionId))
            {
                return UserGroups[Context.ConnectionId];
            }
            return new List<string>();
        }

        private bool GroupExists(string groupName)
        {
            return GroupMessages.ContainsKey(groupName);
        }

        public async Task CreateGroup(string groupName)
        {
            if (!GroupExists(groupName))
            {
                GroupMessages.Add(groupName, new List<string>());
            }
            await Clients.Caller.SendAsync("onMessage", $"Group '{groupName}' created.");
        }

    }









}
