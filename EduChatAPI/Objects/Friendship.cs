using System;
namespace EduChatAPI.Objects
{
    public class Friendship
    {
        public int FirstUserId { get; set; }
        public int SecondUserId { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsBestFriend { get; set; }

        //internal
        public User User1 { get; set; }
        public User User2 { get; set; }
    }
}
