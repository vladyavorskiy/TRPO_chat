namespace TRPO_Web_start
{
    using System.Collections.Generic;
    using static TRPO_Web_start.Controllers.ChatController;

    public interface IChatStateService
    {
        Dictionary<string, string> UserConnections { get; }
        Dictionary<string, List<string>> UserGroups { get; }
        Dictionary<string, List<Message>> GroupMessages { get; }
        Dictionary<string, string> GroupCreators { get; }

    }

    public class ChatStateService : IChatStateService
    {
        public Dictionary<string, string> UserConnections { get; } = new Dictionary<string, string>();
        public Dictionary<string, List<string>> UserGroups { get; } = new Dictionary<string, List<string>>();
        public Dictionary<string, List<Message>> GroupMessages { get; } = new Dictionary<string, List<Message>>();
        public Dictionary<string, string> GroupCreators { get; } = new Dictionary<string, string>(); 

    }
}

