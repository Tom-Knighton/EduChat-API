﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduChatAPI.APITasks;
using EduChatAPI.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EduChatAPI.API
{
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        [HttpPost("CreateNewChat")]
        public async Task<IActionResult> CreateNewChat([FromBody] Chat chat)
        {
            return Ok(await new ChatTasks().CreateNewChat(chat));
        }
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
        [HttpGet("GetMessagesForChat/{ChatId}")]
        public async Task<IActionResult> GetMessagesForChat(int ChatId)
        {
            return Ok(await new ChatTasks().GetMessagesForChat(ChatId));
        }
        [HttpGet("GetChatMessageById/{MessageId}")]
        public async Task<IActionResult> GetMessageById(int MessageId)
        {
            return Ok(await new ChatTasks().GetMessageById(MessageId));
        }

        [HttpPost("AddNewMessageToChat/{ChatId}")]
        public async Task<IActionResult> AddNewMessageToChat(int ChatId, [FromBody] ChatMessage msg)
        {
            return Ok(await new ChatTasks().AddMessageToChat(msg, ChatId));
        }

        [HttpPost("UploadChatAttachment/{ChatId}")]
        public async Task<IActionResult> UploadChatAttachment(int ChatId)
        {
            if (HttpContext.Request.Form.Files.Count > 0) //The body must contain at least 1 file
            {
                return Ok(await new ChatTasks().UploadChatAttachment(HttpContext.Request.Form.Files.First(), ChatId)); //Returns success (200) with the new url
            }
            return BadRequest(); //Else, returns code 400, bad request
        }

        [HttpPut("RemoveChatMessage/{ChatId}/{MessageId}")]
        public async Task<IActionResult> RemoveChatMessage(int ChatId, int MessageId)
        { 
            return Ok(await new ChatTasks().RemoveMessage(MessageId, ChatId)); //Returns success (200) with a boolean indicating success
        }

        [HttpPut("RemoveFromChat/{chatid}/{userid}")]
        public async Task<IActionResult> RemoveUserFromChat(int chatid, int userid)
        {
            return Ok(await new ChatTasks().RemoveFromChat(userid, chatid));
        }
        [HttpPut("ModifyChatName/{chatid}")]
        public async Task<IActionResult> ModifyChatName(int chatid, [FromBody] string name)
        {
            return Ok(await new ChatTasks().ModifyChatName(chatid, name));
        }
    }
}
