using System;
namespace EduChatAPI.Objects
{
    public class ChatMessage
    {
        public int messageId { get; set; }
        public int chatId { get; set; }
        public int userId { get; set; }
        public int messageType { get; set; }
        public string messageText { get; set; }
        public bool hasBeenEdited { get; set; }
        public DateTime dateCreated { get; set; }
        public bool isDeleted { get; set; }
    }
}
