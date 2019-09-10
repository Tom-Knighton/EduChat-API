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
    public class ChatController : Controller
    {

        [HttpGet("GetChatById/{ChatId}")]
        public async Task<IActionResult> GetChatById(int ChatId)
        {
            return Ok(await new ChatTasks().GetChatById(ChatId));
        }
        [HttpGet("GetChatsForUser/{UserId}")]
        public async Task<IActionResult> GetChatsForUser(int UserId)
        {
            return Ok(await new ChatTasks().GetAllChatsForUser(UserId));
        }
    }
}
