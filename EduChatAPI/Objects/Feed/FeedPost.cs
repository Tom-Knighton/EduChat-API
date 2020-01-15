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


    public class FeedPoll : FeedPost // Exends FeedPost
    {
        public string PollQuestion { get; set; }
        public List<FeedAnswer> Answers { get; set; }
    }
    public class FeedAnswer
    {
        public int AnswerId { get; set; }
        public int PostId { get; set; }
        public string Answer { get; set; }
        public List<FeedAnswerVote> Votes { get; set; }
    }
    public class FeedAnswerVote
    {
        public int AnswerId { get; set; }
        public int UserId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
