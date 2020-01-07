using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduChatAPI.APITasks;
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
        [HttpPost("SetLikeForPost/{postid}/{userid}/{like}")]
        public async Task<IActionResult> SetLikeForPost(int postid, int userid, bool like)
        {
            return Ok(await new FeedTasks().SetLikeStatus(postid, userid, like));
        }
    }
}
