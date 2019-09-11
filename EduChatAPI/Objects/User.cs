using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduChatAPI.Objects
{
    public class User
    {
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public string UserProfilePictureURL { get; set; }
        public string UserSchool { get; set; }
        public string UserGender { get; set; }
        public DateTime UserDOB { get; set; }
        public bool IsModerator { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDeleted { get; set; }
        public string UserPassHash { get; set; }

        public List<Subject> Subjects { get; set; }
        public List<Chat> Chats { get; set; }
        
    }   
}
