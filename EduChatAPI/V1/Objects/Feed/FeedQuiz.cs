using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduChatAPI.Objects.Feed
{
    public class FeedQuiz : FeedPost
    {
        public string QuizTitle { get; set; }
        public string DifficultyLevel { get; set; }
        public List<FeedQuizQuestion> Questions { get; set; }
        public List<FeedQuizResult> Results { get; set; }
    }

    public class FeedQuizQuestion
    {
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public int PostId { get; set; }
        public List<string> Answers { get; set; }
        public string CorrectAnswer { get; set; }
        public int Difficulty { get; set; }
    }
    public class FeedQuizResult
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public int OverallScore { get; set; }
        public DateTime DatePosted { get; set; }
        public User User { get; set; }
    }
}
