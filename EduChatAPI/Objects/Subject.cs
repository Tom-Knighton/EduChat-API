using System;
using MySql.Data.MySqlClient;

namespace EduChatAPI.Objects
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public bool IsEducational { get; set; }
        public bool IsAdvanced { get; set; }
        public bool IsEnabled { get; set; }

    
    }


}
