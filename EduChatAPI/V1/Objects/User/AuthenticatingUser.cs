using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduChatAPI.Objects
{
    public class AuthenticatingUser
    {
        public string Authenticator { get; set; }
        public string PassHash { get; set; }
    }
}
