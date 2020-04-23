using System;
namespace EduChatAPI.Objects.Feed
{
    public class FeedLike
    {
        public int UserId { get; set; }
        public int PostId { get; set; }
        public bool IsLiked { get; set; }
    }
}
