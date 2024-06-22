using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static TRPO_Web_start.Controllers.ChatController;

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

        [HttpPost("CreateGroup")]
        public IActionResult CreateGroup(string groupName, string creator)
        {
            if (!_chatStateService.GroupMessages.ContainsKey(groupName))
            {
                _chatStateService.GroupMessages.Add(groupName, new List<Message>());
                
                var creatorInfo = new UsersInfo
                {
                    Name = creator,
                    ChatRole = UserRole.Creator
                };

                _chatStateService.UserInGroup.Add(groupName, new List<UsersInfo> { creatorInfo });
                return Ok($"Group '{groupName}' created.");
            }
            else
            {
                return BadRequest($"Group '{groupName}' already exists.");
            }
        }


        [HttpPost("JoinGroup")]
        public async Task<IActionResult> JoinGroup(string groupName, string userName)
        {
            if (!_chatStateService.GroupMessages.ContainsKey(groupName))
            {
                return BadRequest($"Group '{groupName}' does not exist.");
            }

            if (!_chatStateService.UserConnections.ContainsKey(userName))
            {
                return BadRequest($"User '{userName}' does not exist.");
            }

            string connectionId = _chatStateService.UserConnections[userName];

            if (!_chatStateService.UserGroups.ContainsKey(userName))
            {
                _chatStateService.UserGroups.Add(userName, new List<string>());
            }

            if (!_chatStateService.UserGroups[userName].Contains(groupName))
            {
                await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
                _chatStateService.UserGroups[userName].Add(groupName);
                string message = userName + " вступил в группу.";
                var newMessage = new Message { Text = message, Sender = "System", ReplyTo = null };
                _chatStateService.GroupMessages[groupName].Add(newMessage);
                await _hubContext.Clients.Group(groupName).SendAsync("onUserJoin", userName, newMessage.Id);
            }

            if (_chatStateService.UserInGroup.ContainsKey(groupName))
            {
                var usersInGroup = _chatStateService.UserInGroup[groupName];
                if (!usersInGroup.Any(u => u.Name == userName))
                {
                    usersInGroup.Add(new UsersInfo { Name = userName, ChatRole = UserRole.Common });
                }
            }
            else
            {
                return BadRequest($"Group '{groupName}' does not exist in UserInGroup.");
            }

            foreach (var message in _chatStateService.GroupMessages[groupName])
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("onMessage", message.Text, message.Sender);
            }

            return Ok();
        }


        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string groupName, string userName)
        {
            if (!_chatStateService.UserConnections.ContainsKey(userName))
            {
                return BadRequest($"User '{userName}' does not exist.");
            }

            string connectionId = _chatStateService.UserConnections[userName];

            if (_chatStateService.UserGroups.ContainsKey(userName) && _chatStateService.UserGroups[userName].Contains(groupName) && _chatStateService.UserInGroup.ContainsKey(groupName))
            {

                var usersInGroup = _chatStateService.UserInGroup[groupName];
                var userToRemove = usersInGroup.FirstOrDefault(u => u.Name == userName);
                if (userToRemove != null)
                {
                    usersInGroup.Remove(userToRemove);
                }

                string message = userName + " был удален администрацией";
                var newMessage = new Message { Text = message, Sender = "System", ReplyTo = null };
                _chatStateService.GroupMessages[groupName].Add(newMessage);
                await _hubContext.Clients.Group(groupName).SendAsync("onUserDeleted", groupName, userName, 20);
                _chatStateService.UserGroups[userName].Remove(groupName);
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
            }

            return Ok();
        }



        [HttpPost("LeaveGroup")]
        public async Task<IActionResult> LeaveGroup(string groupName, string userName)
        {
            if (!_chatStateService.UserConnections.ContainsKey(userName))
            {
                return BadRequest($"User '{userName}' does not exist.");
            }

            string connectionId = _chatStateService.UserConnections[userName];

            if (_chatStateService.UserGroups.ContainsKey(userName) && _chatStateService.UserGroups[userName].Contains(groupName) && _chatStateService.UserInGroup.ContainsKey(groupName))
            {

                var usersInGroup = _chatStateService.UserInGroup[groupName];
                var userToRemove = usersInGroup.FirstOrDefault(u => u.Name == userName);
                if (userToRemove != null)
                {
                    usersInGroup.Remove(userToRemove);
                }

                _chatStateService.UserGroups[userName].Remove(groupName);
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
                string message = userName + " покинул группу.";
                var newMessage = new Message { Text = message, Sender = "System", ReplyTo = null };
                _chatStateService.GroupMessages[groupName].Add(newMessage);
                await _hubContext.Clients.Group(groupName).SendAsync("onUserRemoved", userName, newMessage.Id);

            }

            return Ok();
        }


        [HttpPost]
        [Route("SetUserName")]
        public IActionResult SetUserName([FromQuery] string userName, [FromQuery] string connectionId)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(connectionId))
            {
                return BadRequest("Invalid username or connection ID.");
            }

            if (!_chatStateService.UserConnections.ContainsKey(userName))
            {
                _chatStateService.UserConnections[userName] = connectionId;
                return Ok();
            }

            _chatStateService.UserConnections[userName] = connectionId;
            return Ok();
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

        [HttpPost("DeleteGroup")]
        public async Task<IActionResult> DeleteGroup(string groupName, string userName)
        {
            if (_chatStateService.UserInGroup.ContainsKey(groupName))
            {
                var groupUsers = _chatStateService.UserInGroup[groupName];
                var creator = groupUsers.FirstOrDefault(u => u.ChatRole == UserRole.Creator);
                if (creator.Name == userName)
                {
                    _chatStateService.UserInGroup.Remove(groupName);
                }

                if (_chatStateService.GroupMessages.ContainsKey(groupName))
                {
                    _chatStateService.GroupMessages.Remove(groupName);
                }
                await _hubContext.Clients.Group(groupName).SendAsync("onGroupDeleted", groupName);

                var usersInGroup = _chatStateService.UserGroups
                    .Where(ug => ug.Value.Contains(groupName))
                    .Select(ug => ug.Key)
                    .ToList();

                foreach (var user in usersInGroup)
                {
                    _chatStateService.UserGroups[user].Remove(groupName);
                    var connectionId = _chatStateService.UserConnections[user];
                    await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
                }

                return Ok("Group deleted successfully.");
            }
            else
            {
                return BadRequest("You are not the creator of the group.");
            }
        }



        [HttpPost("RemoveMessageForAll")]
        public async Task<IActionResult> RemoveMessageForAll(string groupName, int messageId)
        {
            if (_chatStateService.GroupMessages.ContainsKey(groupName))
            {
                List<Message> messages = _chatStateService.GroupMessages[groupName];
                Message messageToRemove = messages.Find(msg => msg.Id == messageId);
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
                            return BadRequest("Failed to remove message.");
                        }

                    }
                    _chatStateService.GroupMessages[groupName].Remove(messageToRemove);
                    await _hubContext.Clients.Group(groupName).SendAsync("onMessageRemoved", messageId);
                    return Ok("Message removed for all.");
                }
                else
                {
                    return BadRequest("Failed to remove message.");
                }
            }
            return BadRequest("Failed to remove message.");
        }


        [HttpPost("EditMessage")]
        public async Task<IActionResult> EditMessage(string groupName, int messageId, string newText)
        {
            if (_chatStateService.GroupMessages.ContainsKey(groupName))
            {
                List<Message> messages = _chatStateService.GroupMessages[groupName];
                Message messageToEdit = messages.Find(msg => msg.Id == messageId);
                if (messageToEdit != null)
                {
                    messageToEdit.Text = newText;
                    await _hubContext.Clients.Group(groupName).SendAsync("onMessageEdited", messageId, newText);
                    return Ok("Message edited");
                }
                else
                {
                    return BadRequest("Failed to edit message.");
                }
            }
            return BadRequest("Failed to edit message.");
        }


        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage(string groupName, string message, string userName, int? messageId)
        {
            if (!_chatStateService.GroupMessages.ContainsKey(groupName))
            {
                return BadRequest($"Group '{groupName}' does not exist.");
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
                    
                    var newMessageWithReply = new Message { Text = message, Sender = userName};
                    newMessageWithReply.ReplyTo = new Message.ReplyInfo(newReplyMessage.Id, null);
                    
                    _chatStateService.GroupMessages[groupName].Add(newMessageWithReply);
                    await _hubContext.Clients.Group(groupName).SendAsync("onMessage", newMessageWithReply);
                    return Ok();

                }
                else
                {
                    return BadRequest("Failed to edit message.");
                }
            }

            var newMessage = new Message { Text = message, Sender = userName };
            _chatStateService.GroupMessages[groupName].Add(newMessage);

            await _hubContext.Clients.Group(groupName).SendAsync("onMessage", newMessage);

            return Ok();
        }


        [HttpGet("GetGroupCreator")]
        public IActionResult GetGroupCreator(string groupName)
        {
            if (_chatStateService.UserInGroup.ContainsKey(groupName))
            {
                var users = _chatStateService.UserInGroup[groupName];
                var creator = users.FirstOrDefault(u => u.ChatRole == UserRole.Creator);
                if (creator != null)
                {
                    return Ok(creator.Name);
                }
                return BadRequest("Creator not found in the group.");
            }
            return BadRequest("Group does not exist.");
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
            return BadRequest("Group does not exist.");
        }


        [HttpPost("ChangeUserRole")]
        public async Task<IActionResult> ChangeUserRole(string groupName, string userName, UserRole newRole)
        {
            if (_chatStateService.UserInGroup.ContainsKey(groupName))
            {
                var usersInGroup = _chatStateService.UserInGroup[groupName];
                var userToUpdate = usersInGroup.FirstOrDefault(u => u.Name == userName);
                if (userToUpdate != null)
                {
                    userToUpdate.ChatRole = newRole;
                    await _hubContext.Clients.Group(groupName).SendAsync("onUserRoleEdited", groupName, userName, newRole.ToString());
                    return Ok($"Role for user '{userName}' changed to '{newRole}'.");
                }
                else
                {
                    return BadRequest($"User '{userName}' not found in group '{groupName}'.");
                }
            }
            else
            {
                return BadRequest($"Group '{groupName}' does not exist.");
            }
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
                    return BadRequest($"User '{userName}' is not in the group '{groupName}'.");
                }
            }
            return BadRequest("Error");
        }

        public enum UserRole
        {
            DefaultRole,
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