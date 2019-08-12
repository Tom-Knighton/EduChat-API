using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EduChatAPI.APITasks;
using EduChatAPI.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduChatAPI.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {


        [HttpPost("AuthenticateUser")] //Post request so we can upload a body
        public async Task<IActionResult> AuthenticateUser([FromBody] AuthenticatingUser usr) //expects authenticatinguser in body
        {
            User user = await new UserTasks().AuthenticateUser(usr); //Runs AuthenticateUser task
            if (user == null) return NotFound(); //returns not found (404) if the user is null
            else return Ok(user); //else, returns 200 with the user in json
        }
        
        [HttpGet("GetUserById/{userid}")] //Sets the name of the GET request
        public async Task<IActionResult> GetUserById(int userid) //returns a http status code with information
        {
            User user = await new UserTasks().GetUserById(userid); //Creates a new instance of the UserTasks class and gets the user object from the ID
            if (user == null || userid == 0 || userid == null) return NotFound(); //Returns not found if the request is invalid (status code 404)
            return Ok(user); //Else, returns Ok (200) with the user object as json
        }

        [HttpGet("IsUsernameFree/{username}")]
        public async Task<bool> IsUsernameFree(string username)
        {
            return await new UserTasks().IsUsernameFree(username); //Will return either true or false
        }
        [HttpGet("IsEmailFree/{email}")]
        public async Task<bool> IsEmailFree(string email)
        {
            return await new UserTasks().IsEmailFree(email); //Will return either true or false
        }


        [HttpPost("CreateNewUser")]
        public async Task<IActionResult> CreateNewUser([FromBody] User usr) //Expects a user in the body of the request
        {
            return Ok(await new UserTasks().CreateNewUser(usr));//Will return the same user but with updated UserId
        }
        [HttpPost("UploadUserProfilePic/{userid}")]
        public async Task<IActionResult> UploadUserProfilePicture(int userid) //Takes the userid as an argument
        {
            if (HttpContext.Request.Form.Files.Count > 0) //The body must contain at least 1 file
            {
                return Ok(await new UserTasks().UploadProfilePicture(HttpContext.Request.Form.Files[0], userid)); //Returns success (200) with the new user
            }
            else return BadRequest(); //Else, returns code 400, bad request
        }

    }
}
