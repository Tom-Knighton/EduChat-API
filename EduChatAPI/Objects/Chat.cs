using System;
using System.Collections.Generic;

namespace EduChatAPI.Objects
{
    public class Chat
    {
        public int ChatId { get; set; }
        public string ChatName { get; set; }
        public bool isProtected { get; set; }
        public bool isPublic { get; set; }
        public bool isDeleted { get; set; }

        public List<ChatMember> members { get; set; }
        public List<int> memberIds { get; set; }
        public List<ChatMessage> messages { get; set; }
    }
}
