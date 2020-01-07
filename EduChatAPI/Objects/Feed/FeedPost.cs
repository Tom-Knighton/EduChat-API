using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EduChatAPI.Objects.Feed
{
    public class FeedPost
    {
        public int postId { get; set; }
        public string postType { get; set; }
        public int posterId { get; set; }
        public int subjectId { get; set; }
        public DateTime datePosted { get; set; }
        public bool isAnnouncement { get; set; }
        public bool isDeleted { get; set; }

        public List<FeedLike> likes { get; set; }
        public User poster { get; set; }
        public Subject subject { get; set; }
    }


    public class FeedMediaPost : FeedPost //extends FeedPost
    {
        public string urlToPost { get; set; }
        public string postDescription { get; set; }
        public bool isVideo { get; set; }
    }

    public class FeedTextPost : FeedPost //Extends FeedPost
    {
        public string postText { get; set; }
    }
}
