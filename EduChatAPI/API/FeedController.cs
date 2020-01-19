using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduChatAPI.APITasks;
using EduChatAPI.Objects.Feed;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EduChatAPI.API
{
    [Route("api/[controller]")]
    public class FeedController : Controller
    {
        [HttpGet("GetPostById/{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            return Ok(await new FeedTasks().GetPostById(id));
        }
        [HttpGet("GetAllPostsForSubject/{subjectId}")]
        public async Task<IActionResult> GetAllPostsForSubject(int subjectid)
        {
            return Ok(await new FeedTasks().GetAllPostsForSubject(subjectid));
        }
        [HttpGet("GetAllCommentsForPost/{PostId}")]
        public async Task<IActionResult> GetAlLCommentsForPost(int PostId)
        {
            return Ok(await new FeedTasks().GetAllCommentsForPost(PostId));
        }
        [HttpPut("SetLikeForPost/{postid}/{userid}/{like}")]
        public async Task<IActionResult> SetLikeForPost(int postid, int userid, bool like)
        {
            return Ok(await new FeedTasks().SetLikeStatus(postid, userid, like));
        }
        [HttpPost("CreateCommentForPost/{postid}/{userid}")]
        public async Task<IActionResult> CreateCommentForPost(int postid, int userid, [FromBody] FeedComment comment)
        {
            return Ok(await new FeedTasks().CreateNewCommentForPost(postid, comment, userid));
        }

        [HttpPost("UploadFeedMediaAttachment")]
        public async Task<IActionResult> UploadFeedMediaAttachment()
        {
            if (HttpContext.Request.Form.Files.Count > 0) { return Ok(await new FeedTasks().UploadFeedMediaAttachment(HttpContext.Request.Form.Files[0])); }
            return BadRequest();
        }

        [HttpPost("UploadTextPost")]
        public async Task<IActionResult> UploadTextPost([FromBody] FeedTextPost post)
        {
            return Ok(await new FeedTasks().UploadTextPost(post));
        }
        [HttpPost("UploadMediaPost")]
        public async Task<IActionResult> UploadMediaPost([FromBody] FeedMediaPost post)
        {
            return Ok(await new FeedTasks().UploadMediaPost(post));
        }


        //Poll:
        [HttpPut("VoteForPoll/{userid}/{answerid}/{pollid}")]
        public async Task<IActionResult> VoteForPoll(int userid, int answerid, int pollid)
        {
            return Ok(await new FeedTasks().VoteForPoll(userid, answerid, pollid));
        }
        [HttpPost("UploadPoll")]
        public async Task<IActionResult> UploadPoll([FromBody] FeedPoll poll)
        {
            return Ok(await new FeedTasks().UploadPollPost(poll));
        }

        //Quiz:
        [HttpGet("GetFullFeedQuiz/{QuizId}")]
        public async Task<IActionResult> GetFullFeedQuiz(int QuizId)
        {
            return Ok(await new FeedTasks().GetFullFeedQuiz(QuizId));
        }

    }
}
