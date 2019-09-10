using System;
namespace EduChatAPI.Objects
{
    public class ChatMember
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public bool isInChat { get; set; }

        public User user { get; set; }
    }
}
