using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EduChatAPI.Objects;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;

namespace EduChatAPI.APITasks
{
    public class ChatTasks
    {
        string connString= "server=127.0.0.1;port=3306;database=educhat;user=db;password=Tom7494";


        public async Task<Chat> CreateNewChat(Chat chat)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = $"INSERT INTO chat VALUES({0}, '{chat.ChatName}', {chat.isProtected}, {chat.isPublic}, {chat.isDeleted}, {chat.DateCreated.ToString("yyyy-MM-dd hh:mm:ss")});";
                    await cmd.ExecuteNonQueryAsync();
                    chat.ChatId = (int)cmd.LastInsertedId;
                    foreach (int m in chat.memberIds) { await AddToChat(m, chat.ChatId); }
                    return await GetChatById(chat.ChatId);
                }

            }
        }
        public async Task<Chat> GetChatById(int chatid)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using(var cmd = new MySqlCommand($"SELECT * FROM chat WHERE chatId={chatid}", conn))
                using(var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync())
                    {
                        Chat chat = new Chat
                        {
                            ChatId = Convert.ToInt32(reader["chatId"]),
                            ChatName = reader["chatName"].ToString(),
                            isProtected = Convert.ToBoolean(reader["isProtected"]),
                            isPublic = Convert.ToBoolean(reader["isPublic"]),
                            isDeleted = Convert.ToBoolean(reader["isDeleted"]),
                            DateCreated = Convert.ToDateTime(reader["dateCreated"]),
                            members = await GetMembersForChat(chatid),
                            LastMessage = await GetLastChatMessageForChat(chatid)
                            
                        };
                        List<int> memberids = new List<int>();
                        foreach (ChatMember m in chat.members) { if (m.isInChat) { memberids.Add(m.UserId); } }
                        chat.memberIds = memberids.OrderBy(i => i).ToList();
                        return chat;
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
                chats.OrderBy(c => c.messages.Count > 0 ? c.messages.Last().dateCreated : c.DateCreated);
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
                            user = await new UserTasks().GetUserById(Convert.ToInt32(reader["userId"]), flatten: true)
                        });
                    }
                return members;
            }
        }


        public async Task<ChatMessage> GetLastChatMessageForChat(int chatid)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand($"SELECT * FROM chat_message WHERE chatId={chatid} AND isDeleted={false} ORDER BY dateCreated DESC LIMIT 1;", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync())
                        return await GetMessageById(Convert.ToInt32(reader["messageId"]), flatten: true);
                return null;
            }
        }
        public async Task<List<ChatMessage>> GetMessagesForChat(int chatid, bool flatten = false)
        {
            using(var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                List<ChatMessage> messages = new List<ChatMessage>();
                using (var cmd = new MySqlCommand($"SELECT * FROM chat_message WHERE chatId={chatid} AND isDeleted={false};", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync())
                    {
                        messages.Add(new ChatMessage
                        {
                            chatId = Convert.ToInt32(reader["chatId"]),
                            messageId = Convert.ToInt32(reader["messageId"]),
                            messageType = Convert.ToInt32(reader["messageType"]),
                            userId = Convert.ToInt32(reader["userId"]),
                            dateCreated = Convert.ToDateTime(reader["dateCreated"]),
                            hasBeenEdited = Convert.ToBoolean(reader["hasBeenEdited"]),
                            isDeleted = Convert.ToBoolean(reader["isDeleted"]),
                            user = !flatten ? await new UserTasks().GetUserById(Convert.ToInt32(reader["userId"]), flatten: true) : null,
                            messageText = reader["messageText"].ToString()
                        });
                    }
                return messages;
            }
        }

        public async Task<ChatMessage> GetMessageById(int ChatId, bool flatten = false)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand($"SELECT * FROM chat_message WHERE messageId={ChatId};", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync())
                        return new ChatMessage
                        {
                            chatId = Convert.ToInt32(reader["chatId"]),
                            messageId = Convert.ToInt32(reader["messageId"]),
                            messageType = Convert.ToInt32(reader["messageType"]),
                            userId = Convert.ToInt32(reader["userId"]),
                            dateCreated = Convert.ToDateTime(reader["dateCreated"]),
                            hasBeenEdited = Convert.ToBoolean(reader["hasBeenEdited"]),
                            isDeleted = Convert.ToBoolean(reader["isDeleted"]),
                            user = !flatten ? await new UserTasks().GetUserById(Convert.ToInt32(reader["userId"]), flatten: true) : null,
                            messageText = reader["messageText"].ToString()
                        };
                return null;
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
                    cmd.CommandText = $"INSERT INTO chat_member VALUES ({chatid}, {userid}, {true}) ON DUPLICATE KEY UPDATE chatId=VALUES(chatId), userId=VALUES(userId), isInChat=VALUES(isInChat);";
                    await cmd.ExecuteNonQueryAsync();
                    return await GetChatById(chatid);
                }

            }
        }
        public async Task<Chat> RemoveFromChat(int userid, int chatid)
        {
            using (var conn = new MySqlConnection(connString)) //creates temp connection
            {
                await conn.OpenAsync(); //Waits to open
                using (var cmd = new MySqlCommand()) //new cmd
                {
                    cmd.Connection = conn; //sets conenction
                    cmd.CommandText = $"INSERT INTO chat_member VALUES ({chatid}, {userid}, {false}) ON DUPLICATE KEY UPDATE chatId=VALUES(chatId), userId=VALUES(userId), isInChat=VALUES(isInChat);";
                    await cmd.ExecuteNonQueryAsync(); //< executes ^ 
                    return await GetChatById(chatid); //returns new chat object
                }

            }
        }

        public async Task<ChatMessage> AddMessageToChat(ChatMessage msg, int chatId)
        {
            using (var conn = new MySqlConnection(connString)) //Creates connection that will be destroyed when scope ends
            {
                await conn.OpenAsync(); //Waits for connection to open
                using (var cmd = new MySqlCommand()) //New command
                {
                    cmd.Connection = conn; //Sets connection
                    cmd.CommandText = $"INSERT INTO chat_message VALUES ({0}, {chatId}, {msg.userId}, {msg.messageType}, '{msg.messageText}', {false}, '{msg.dateCreated.ToString("yyyy-MM-dd hh:mm:ss")}', {false});";
                    // ^ Inserts new message into chat_message table
                    await cmd.ExecuteNonQueryAsync(); //waits for command to execute
                    int lId = (int)cmd.LastInsertedId; //Grabs the auto incremented Id of that message 
                    msg.messageId = lId; return msg; //Returns the message with that id
                }
            }
        }

        public async Task<bool> RemoveMessage(int messageId, int chatId)
        {
            using (var conn = new MySqlConnection(connString)) // Creates connection that will be destroyed when scope ends
            {
                await conn.OpenAsync(); //Waits for connection to open
                using (var cmd = new MySqlCommand()) //New command
                {
                    cmd.Connection = conn; //Sets connection
                    cmd.CommandText = $"UPDATE chat_message SET `isDeleted`='1' WHERE `messageId`={messageId}";
                    // ^ sets is deleted to true where the message is
                    await cmd.ExecuteNonQueryAsync(); //executes command
                    return true; //returns true
                }
            }
        }
        public async Task<String> UploadChatAttachment(IFormFile file, int chatId)
        {
            Directory.CreateDirectory($"/var/www/cdn/ChatAttachments/{chatId}/"); //Creates the directory if it does not exist
            var filepath = $"/var/www/cdn/ChatAttachments/{chatId}/{file.FileName}";
            using (var stream = new FileStream(filepath, FileMode.Create)) //the using keyword means this FileStream will be destroyed once it has completed
            {
                await file.CopyToAsync(stream); //Creates the file at the filePath;
            }
            return $"https://cdn.tomk.online/ChatAttachments/{chatId}/{file.FileName}";
        }

        public async Task<Chat> ModifyChatName(int chatid, string name)
        {
            using (var conn = new MySqlConnection(connString)) //Creates new connection
            {
                await conn.OpenAsync(); //Waits to open
                using (var cmd = new MySqlCommand($"UPDATE chat SET `chatName` = '{name}' WHERE `chatId`={chatid};", conn))
                { //^ Updates chatName on the specified row
                    await cmd.ExecuteNonQueryAsync(); //waits for cmd to execute
                    return await GetChatById(chatid); //Returns the new chat object
                }
            }
        }
    }
}
