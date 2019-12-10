using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduChatAPI.APITasks;
using EduChatAPI.Objects;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EduChatAPI.API
{
    [Route("api/[controller]")]
    public class FriendshipController : Controller
    {
        [HttpPost("CreateFriendship")]
        public async Task<IActionResult> CreateFriendsip([FromBody] Friendship friendship)
        {
            return Ok(await new FriendshipTasks().CreateFriendship(friendship));
        }

        [HttpGet("DoesFriendshipExist/{userid1}/{userid2}")]
        public async Task<IActionResult> DoesFriendshipExist(int userid1, int userid2)
        {
            return Ok(await new FriendshipTasks().DoesFriendshipExist(userid1, userid2));
        }

        [HttpGet("IsBlocked/{userid1}/{userid2}")]
        public async Task<IActionResult> IsBlocked(int userid1, int userid2)
        {
            return Ok(await new FriendshipTasks().IsBlocked(userid1, userid2));
        }

        [HttpPut("SetBlock/{userid1}/{userid2}")]
        public async Task<IActionResult> SetBlock(int userid1, int userid2, bool block)
        { 
            return Ok(await new FriendshipTasks().SetBlock(userid1, userid2, block));
        }

        [HttpGet("GetAllFriendsForUser/{userId}")]
        public async Task<IActionResult> GetAllFriendsForUser(int userId)
        {
            return Ok(await new FriendshipTasks().GetAllFriendsForUser(userId));
        }

        [HttpGet("RemoveFriendship/{userid1}/{userid2}")]
        public async Task<IActionResult> RemoveFriendship(int userid1, int userid2)
        {
            return Ok(await new FriendshipTasks().RemoveFriendship(userid1, userid2));
        }
    }
}
