using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduChatAPI.APITasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduChatAPI.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        
        [HttpGet("GetUserById/{userid}")] //Sets the name of the GET request
        public IActionResult GetUserById(int userid) //returns a http status code with information
        {
            UserTasks userTasks = new UserTasks(); //Creates a new instance of the UserTasks class
            if (userid == null || userid == 0) return BadRequest(); //If the userid is null return 400
            return Ok(userTasks.GetUserById(userid)); //Else, return 200, with the user object or null if not found
        }

    }
}
