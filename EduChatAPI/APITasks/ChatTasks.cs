using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EduChatAPI.Objects;
using MySql.Data.MySqlClient;

namespace EduChatAPI.APITasks
{
    public class ChatTasks
    {
        string connString= "server=127.0.0.1;port=3306;database=educhat;user=db;password=Tom7494";
        public async Task<Chat> GetChatById(int chatid)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using(var cmd = new MySqlCommand($"SELECT * FROM chat WHERE chatId={chatid}", conn))
                using(var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync())
                    {
                        return new Chat
                        {
                            ChatId = Convert.ToInt32(reader["chatId"]),
                            ChatName = reader["chatName"].ToString(),
                            isProtected = Convert.ToBoolean(reader["isProtected"]),
                            isPublic = Convert.ToBoolean(reader["isPublic"]),
                            isDeleted = Convert.ToBoolean(reader["isDeleted"]),
                            members = await GetMembersForChat(chatid)
                        };
                    }
                return null;
            }
        }

        public async Task<List<Chat>> GetAllChatsForUser(int userid)
        {
            using(var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                List<Chat> chats = new List<Chat>(); //Creates an empty list of chats
                using (var cmd = new MySqlCommand($"SELECT * FROM chat_member INNER JOIN chat ON chat_member.chatId = chat.chatId WHERE chat_member.userId={userid} AND chat_member.isInChat={true} AND chat.isDeleted={false};", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync()) 
                        chats.Add(await GetChatById(Convert.ToInt32(reader["chatId"])));
                return chats;
            }           
        }

        public async Task<List<ChatMember>> GetMembersForChat(int chatid)
        {
            using(var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                List<ChatMember> members = new List<ChatMember>();
                using (var cmd = new MySqlCommand($"SELECT * FROM chat_member WHERE chatId={chatid} AND isInChat={true};", conn))
                using(var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync())
                    {
                        members.Add(new ChatMember
                        {
                            ChatId = Convert.ToInt32(reader["chatId"]),
                            UserId = Convert.ToInt32(reader["userId"]),
                            isInChat = Convert.ToBoolean(reader["isInChat"]),
                            user = await new UserTasks().GetUserById(Convert.ToInt32(reader["userId"]))
                        });
                    }
                return members;
            }
        }

        public async Task<Chat> AddToChat(int userid, int chatid)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO chat_member VALUES ({chatid}, {userid}, {true}) ON DUPLICATE KEY UPDATE chatId=VALUES(chatId), userId=VALUES(userId), isInChat=VALUES(isInChat);";
                    await cmd.ExecuteNonQueryAsync();
                    return await GetChatById(chatid);
                }

            }
        }
    }
}
