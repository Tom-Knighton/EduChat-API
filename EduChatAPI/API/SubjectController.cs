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
    public class SubjectController : Controller
    {
        [HttpGet("GetAllSubjects")]
        public async Task<IActionResult> GetAllSubjectsAsync(bool excludeALevels = false, bool excludeNonEducational = false)
        { 
           return Ok(await new SubjectTasks().GetAllSubjects(excludeALevels, excludeNonEducational));
        }

        [HttpGet("GetSubjectById/{id}")]
        public async Task<IActionResult> GetSubjectById(int id)
        {
            return Ok(await new SubjectTasks().GetSubjectById(id));
        }

    }
}
