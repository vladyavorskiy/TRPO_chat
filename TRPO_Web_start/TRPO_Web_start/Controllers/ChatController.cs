using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;


namespace TRPO_Web_start.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<MainHub> _hubContext;
        private readonly IChatStateService _chatStateService;
        public ChatController(IHubContext<MainHub> hubContext, IChatStateService chatStateService)
        {
            _hubContext = hubContext;
            _chatStateService = chatStateService;
        }

        [HttpPost("SetUserName")]
        public IActionResult SetUserName(string userName, string connectionId)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(connectionId))
            {
                return BadRequest("Invalid username or connection ID");
            }

            if (!_chatStateService.UserConnections.ContainsKey(userName))
            {
                _chatStateService.UserGroups.Add(userName, new List<string>());
                _chatStateService.UserConnections[userName] = connectionId;
                return Ok();
            }
            _chatStateService.UserConnections[userName] = connectionId;
            return Ok();
        }


        [HttpPost("CreateGroup")]
        public IActionResult CreateGroup(string createGroupName, string creatorName)
        {
            if (!_chatStateService.GroupMessages.ContainsKey(createGroupName))
            {
                _chatStateService.GroupMessages.Add(createGroupName, new List<Message>());
                
                var creatorInfo = new UsersInfo
                {
                    Name = creatorName,
                    ChatRole = UserRole.Creator
                };

                _chatStateService.UserInGroup.Add(createGroupName, new List<UsersInfo> { creatorInfo });
                return Ok($"Group '{createGroupName}' created");
            }

            return BadRequest($"Group '{createGroupName}' already exists");
        }


        [HttpPost("JoinGroup")]
        public async Task<IActionResult> JoinGroup(string joinGroupName, string joinUserName)
        {
            if (!_chatStateService.GroupMessages.ContainsKey(joinGroupName) || !_chatStateService.UserInGroup.ContainsKey(joinGroupName))
            {
                return BadRequest($"Group '{joinGroupName}' does not exist");
            }

            if (!_chatStateService.UserConnections.ContainsKey(joinUserName))
            {
                return BadRequest($"User '{joinUserName}' does not exist");
            }

            string connectionId = _chatStateService.UserConnections[joinUserName];


            if (!_chatStateService.UserGroups[joinUserName].Contains(joinGroupName))
            {
                await _hubContext.Groups.AddToGroupAsync(connectionId, joinGroupName);
                _chatStateService.UserGroups[joinUserName].Add(joinGroupName);
                string message = joinUserName + " вступил в группу.";
                var newMessage = new Message { Text = message, Sender = "System", ReplyTo = null };
                _chatStateService.GroupMessages[joinGroupName].Add(newMessage);
                await _hubContext.Clients.Group(joinGroupName).SendAsync("onUserJoin", joinUserName, newMessage.Id);
            }

            if (_chatStateService.UserInGroup.ContainsKey(joinGroupName))
            {
                var usersInGroup = _chatStateService.UserInGroup[joinGroupName];
                if (!usersInGroup.Any(u => u.Name == joinUserName))
                {
                    usersInGroup.Add(new UsersInfo { Name = joinUserName, ChatRole = UserRole.Common });
                }
            }
            else
            {
                return BadRequest($"Group '{joinGroupName}' does not exist");
            }

            foreach (var message in _chatStateService.GroupMessages[joinGroupName])
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("onMessage", message.Text, message.Sender);
            }

            return Ok();
        }


        [HttpPost("LeaveGroup")]
        public async Task<IActionResult> LeaveGroup(string leaveGroupName, string leaveUserName)
        {
            if(!_chatStateService.GroupMessages.ContainsKey(leaveGroupName) || !_chatStateService.UserInGroup.ContainsKey(leaveGroupName))
            {
                return BadRequest($"Group '{leaveGroupName}' does not exist");
            }

            if (!_chatStateService.UserConnections.ContainsKey(leaveUserName))
            {
                return BadRequest($"User '{leaveUserName}' does not exist");
            }

            string connectionId = _chatStateService.UserConnections[leaveUserName];

            if (_chatStateService.UserGroups.ContainsKey(leaveUserName) && _chatStateService.UserGroups[leaveUserName].Contains(leaveGroupName))
            {
                //можно изменить реализацию
                var usersInGroup = _chatStateService.UserInGroup[leaveGroupName];
                var userToRemove = usersInGroup.FirstOrDefault(u => u.Name == leaveUserName);
                if (userToRemove != null)
                {
                    usersInGroup.Remove(userToRemove);
                }

                _chatStateService.UserGroups[leaveUserName].Remove(leaveGroupName);
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, leaveGroupName);
                string message = leaveUserName + " покинул группу.";
                var newMessage = new Message { Text = message, Sender = "System", ReplyTo = null };
                _chatStateService.GroupMessages[leaveGroupName].Add(newMessage);
                await _hubContext.Clients.Group(leaveGroupName).SendAsync("onUserRemoved", leaveUserName, newMessage.Id);

            }

            return Ok();
        }


        [HttpPost("DeleteGroup")]
        public async Task<IActionResult> DeleteGroup(string deleteGroupName, string executorUserName)
        {
            if (_chatStateService.UserInGroup.ContainsKey(deleteGroupName) && _chatStateService.GroupMessages.ContainsKey(deleteGroupName))
            {
                var groupUsers = _chatStateService.UserInGroup[deleteGroupName];
                var creator = groupUsers.FirstOrDefault(u => u.ChatRole == UserRole.Creator);
                if (creator.Name == executorUserName)
                {
                    _chatStateService.UserInGroup.Remove(deleteGroupName);
                    _chatStateService.GroupMessages.Remove(deleteGroupName);
                }

                await _hubContext.Clients.Group(deleteGroupName).SendAsync("onGroupDeleted", deleteGroupName);


                foreach (var user in groupUsers)
                {
                    if (_chatStateService.UserGroups.ContainsKey(user.Name))
                    {
                        var userGroups = _chatStateService.UserGroups[user.Name];
                        userGroups.Remove(deleteGroupName);

                        if (_chatStateService.UserConnections.ContainsKey(user.Name))
                        {
                            var connectionId = _chatStateService.UserConnections[user.Name];
                            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, deleteGroupName);
                        }
                    }
                }


                return Ok($"Group '{deleteGroupName}' deleted successfully");
            }

            return BadRequest("You are not the creator of the group");
        }
        

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage(string groupName, string message, string userName, int? messageId)
        {
            if (!_chatStateService.GroupMessages.ContainsKey(groupName))
            {
                return BadRequest($"Group '{groupName}' does not exist");
            }

            if (messageId != null)
            {
                List<Message> messages = _chatStateService.GroupMessages[groupName];
                Message messageToReply = messages.Find(msg => msg.Id == messageId);
                if (messageToReply != null)
                {
                    var newReplyMessage = new Message { Text = messageToReply.Text, Sender = messageToReply.Sender };
                    if (messageToReply.Sender == userName)
                    {
                        newReplyMessage.ReplyTo = new Message.ReplyInfo(messageToReply.Id, true);
                    }
                    else
                    {
                        newReplyMessage.ReplyTo = new Message.ReplyInfo(messageToReply.Id, false);
                    }
                    _chatStateService.GroupMessages[groupName].Add(newReplyMessage);
                    await _hubContext.Clients.Group(groupName).SendAsync("onMessage", newReplyMessage);

                    var newMessageWithReply = new Message { Text = message, Sender = userName };
                    newMessageWithReply.ReplyTo = new Message.ReplyInfo(newReplyMessage.Id, null);

                    _chatStateService.GroupMessages[groupName].Add(newMessageWithReply);
                    await _hubContext.Clients.Group(groupName).SendAsync("onMessage", newMessageWithReply);
                    return Ok();

                }

                return BadRequest("Failed to reply message");
            }

            var newMessage = new Message { Text = message, Sender = userName };
            _chatStateService.GroupMessages[groupName].Add(newMessage);

            await _hubContext.Clients.Group(groupName).SendAsync("onMessage", newMessage);

            return Ok();
        }


        [HttpPost("EditMessage")]
        public async Task<IActionResult> EditMessage(string groupName, int editMessageId, string newText)
        {
            if (_chatStateService.GroupMessages.ContainsKey(groupName))
            {
                List<Message> messages = _chatStateService.GroupMessages[groupName];
                Message messageToEdit = messages.Find(msg => msg.Id == editMessageId);
                if (messageToEdit != null)
                {
                    messageToEdit.Text = newText;
                    await _hubContext.Clients.Group(groupName).SendAsync("onMessageEdited", editMessageId, newText);
                    return Ok();
                }

                return BadRequest("Failed to edit message");
            }
            return BadRequest($"Group '{groupName}' does not exist");
        }


        [HttpPost("RemoveMessageForAll")]
        public async Task<IActionResult> RemoveMessageForAll(string groupName, int removeMessageId)
        {
            if (_chatStateService.GroupMessages.ContainsKey(groupName))
            {
                List<Message> messages = _chatStateService.GroupMessages[groupName];
                Message messageToRemove = messages.Find(msg => msg.Id == removeMessageId);
                if (messageToRemove != null)
                {
                    if (messageToRemove.ReplyTo?.replyState == null && messageToRemove.ReplyTo != null)
                    {

                        Message messageReplyToRemove = messages.Find(msg => msg.Id == messageToRemove.ReplyTo.messageReplyId);
                        if (messageReplyToRemove != null)
                        {
                            _chatStateService.GroupMessages[groupName].Remove(messageReplyToRemove);
                            await _hubContext.Clients.Group(groupName).SendAsync("onMessageRemoved", messageToRemove.ReplyTo.messageReplyId);
                        }
                        else
                        {
                            return BadRequest("Failed to remove message");
                        }

                    }
                    _chatStateService.GroupMessages[groupName].Remove(messageToRemove);
                    await _hubContext.Clients.Group(groupName).SendAsync("onMessageRemoved", removeMessageId);
                    return Ok();
                }

                return BadRequest("Failed to remove message");
            }
            return BadRequest($"Group '{groupName}' does not exist");
        }


        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string groupName, string deleteUserName)
        {
            if (!_chatStateService.GroupMessages.ContainsKey(groupName) || !_chatStateService.UserInGroup.ContainsKey(groupName))
            {
                return BadRequest($"Group '{groupName}' does not exist");
            }

            if (!_chatStateService.UserConnections.ContainsKey(deleteUserName))
            {
                return BadRequest($"User '{deleteUserName}' does not exist");
            }

            string connectionId = _chatStateService.UserConnections[deleteUserName];

            if (_chatStateService.UserGroups.ContainsKey(deleteUserName) && _chatStateService.UserGroups[deleteUserName].Contains(groupName))
            {
                var usersInGroup = _chatStateService.UserInGroup[groupName];
                var userToRemove = usersInGroup.FirstOrDefault(u => u.Name == deleteUserName);
                if (userToRemove != null)
                {
                    usersInGroup.Remove(userToRemove);
                }

                string message = deleteUserName + " был удален администрацией";
                var newMessage = new Message { Text = message, Sender = "System", ReplyTo = null };
                _chatStateService.GroupMessages[groupName].Add(newMessage);
                await _hubContext.Clients.Group(groupName).SendAsync("onUserDeleted", groupName, deleteUserName, 20);
                _chatStateService.UserGroups[deleteUserName].Remove(groupName);
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
            }

            return Ok();
        }


        [HttpPost("ChangeUserRole")]
        public async Task<IActionResult> ChangeUserRole(string groupName, string changeRoleUserName, UserRole newRole)
        {
            if (_chatStateService.UserInGroup.ContainsKey(groupName))
            {
                var usersInGroup = _chatStateService.UserInGroup[groupName];
                var userToUpdate = usersInGroup.FirstOrDefault(u => u.Name == changeRoleUserName);
                if (userToUpdate != null)
                {
                    userToUpdate.ChatRole = newRole;
                    await _hubContext.Clients.Group(groupName).SendAsync("onUserRoleEdited", groupName, changeRoleUserName, newRole.ToString());
                    return Ok($"Role for user '{changeRoleUserName}' changed to '{newRole}'");
                }

                return BadRequest($"User '{changeRoleUserName}' not found in group '{groupName}'");
            }

            return BadRequest($"Group '{groupName}' does not exist");
        }


        [HttpGet("GetUserGroups")]
        public IActionResult GetUserGroups(string userName)
        {
            if (_chatStateService.UserGroups.ContainsKey(userName))
            {
                return Ok(_chatStateService.UserGroups[userName]);
            }
            return Ok(new List<string>());
        }


        [HttpGet("GetGroupMessages")]
        public IActionResult GetGroupMessages(string groupName)
        {
            if (_chatStateService.GroupMessages.ContainsKey(groupName))
            {
                return Ok(_chatStateService.GroupMessages[groupName]);
            }
            else
            {
                return Ok(new List<string>());
            }
        }


        [HttpGet("GetUsersInGroup")]
        public IActionResult GetUsersInGroup(string groupName)
        {
            if (_chatStateService.UserInGroup.ContainsKey(groupName))
            {
                var users = _chatStateService.UserInGroup[groupName];
                var usersInfo = users.Select(u => new { u.Name, Role = u.ChatRole.ToString() }).ToList();
                return Ok(usersInfo);
            }
            return BadRequest("Group does not exist");
        }


        [HttpGet("GetUsersRole")]
        public IActionResult GetUsersRole(string groupName, string userName)
        {
            if (_chatStateService.UserGroups.ContainsKey(userName) && _chatStateService.UserGroups[userName].Contains(groupName) && _chatStateService.UserInGroup.ContainsKey(groupName))
            {

                var usersInGroup = _chatStateService.UserInGroup[groupName];
                var user = usersInGroup.FirstOrDefault(u => u.Name == userName);

                if (user != null)
                {
                    return Ok(user.ChatRole.ToString());
                }
                else
                {
                    return BadRequest($"User '{userName}' is not in the group '{groupName}'");
                }
            }
            return BadRequest("Error get user role");
        }


        public enum UserRole
        {
            Common,
            Admin,
            Moderator,
            Creator
        }

        public class UsersInfo
        {
            public string Name;
            public UserRole ChatRole;
        }

        public class Message
        {
            private static int _idCounter = 0;
            public int Id { get; private set; }
            public string Text { get; set; }
            public string Sender { get; set; }

            public Message()
            {
                Id = ++_idCounter;
                ReplyTo = null;
            }

            public ReplyInfo ReplyTo { get; set; }

            public class ReplyInfo
            {
                public int messageReplyId { get; set; }
                public bool? replyState { get; set; }


                public ReplyInfo(int replyId, bool? state)
                {
                    messageReplyId = replyId;
                    replyState = state;
                }
            }

        }

    }
}