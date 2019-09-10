using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EduChatAPI.Objects;
using MySql.Data.MySqlClient;

namespace EduChatAPI.APITasks
{
    public class ChatTasks
    {
        MySqlConnection conn = new MySqlConnection("server=127.0.0.1;port=3306;database=educhat;user=db;password=Tom7494");

        public async Task<Chat> GetChatById(int chatid)
        {
            if (conn.State != System.Data.ConnectionState.Open) { await conn.OpenAsync(); } //Opens the connection
            MySqlDataReader reader = new MySqlCommand($"SELECT * FROM chat WHERE chatId={chatid}", conn).ExecuteReader();
            // ^ selects all values from row in chat with matching chatid
            if (reader.Read()) //If it exists
            {
                Chat chat = new Chat //Creates a new chat from the variables returned
                {
                    ChatId = Convert.ToInt32(reader["chatId"]),
                    ChatName = reader["chatName"].ToString(),
                    isProtected = Convert.ToBoolean(reader["isProtected"]),
                    isPublic = Convert.ToBoolean(reader["isPublic"]),
                    isDeleted = Convert.ToBoolean(reader["isDeleted"])
                };
                conn.Close(); // CLoses the connection and returns the chat
                return chat;
            }
            else { conn.Close(); return null; } //If it doesnt exist, close and return null
        }

        public async Task<List<Chat>> GetAllChatsForUser(int userid)
        {
            List<Chat> chats = new List<Chat>(); //Creates an empty list of chats
            if (conn.State != System.Data.ConnectionState.Open) { await conn.OpenAsync(); } //Opens the connection
            MySqlDataReader reader = new MySqlCommand($"SELECT * FROM chat_member INNER JOIN chat ON chat_member.chatId = chat.chatId WHERE chat_member.userId={userid} AND chat_member.isInChat={true} AND chat.isDeleted={false};", conn).ExecuteReader();
            // Selects all from chatmember where userid matches, and inner joins the chat from the chat_member link
            while (reader.Read()) //For each row
            {
                chats.Add(new Chat //Creates new chat from variables
                {
                    ChatId = Convert.ToInt32(reader["chatId"]),
                    ChatName = reader["chatName"].ToString(),
                    isProtected = Convert.ToBoolean(reader["isProtected"]),
                    isPublic = Convert.ToBoolean(reader["isPublic"]),
                    isDeleted = Convert.ToBoolean(reader["isDeleted"]),
                });
            }
            conn.Close(); //closes conenction
            foreach (Chat chat in chats) { chat.members = await GetMembersForChat(chat.ChatId); }
            return chats; //return list
        }

        public async Task<List<ChatMember>> GetMembersForChat(int chatid)
        {
            List<ChatMember> members = new List<ChatMember>();
            if (conn.State != System.Data.ConnectionState.Open) { await conn.OpenAsync(); }
            MySqlDataReader reader = new MySqlCommand($"SELECT * FROM chat_member WHERE chatId={chatid} AND isInChat={true};", conn).ExecuteReader();
            while (reader.Read())
            {
                members.Add(new ChatMember
                {
                    ChatId = Convert.ToInt32(reader["chatId"]),
                    UserId = Convert.ToInt32(reader["userId"]),
                    isInChat = Convert.ToBoolean(reader["isInChat"]),
                });
            }
            conn.Close();
            foreach (ChatMember member in members) member.user = await new UserTasks().GetUserById(member.UserId);
            return members;
        }

        public async Task<Chat> AddToChat(int userid, int chatid)
        {
            if (conn.State != System.Data.ConnectionState.Open) { await conn.OpenAsync(); }
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO chat_member VALUES ({chatid}, {userid}, {true}) ON DUPLICATE KEY UPDATE chatId=VALUES(chatId), userId=VALUES(userId), isInChat=VALUES(isInChat);", conn);
            cmd.ExecuteNonQuery();
            conn.Close();
            return await GetChatById(chatid);
        }
    }
}
