using System;
namespace EduChatAPI.Objects.Feed
{
    public class FeedComment
    {
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public int PostId { get; set; }
        public string Comment { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DatePosted { get; set; }

        public User user { get; set; }
    }

}