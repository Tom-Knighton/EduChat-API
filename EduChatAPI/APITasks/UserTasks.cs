using EduChatAPI.Objects;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EduChatAPI.APITasks
{
    public class UserTasks
    {
       // MySqlConnection conn = new MySqlConnection("server=127.0.0.1;port=3306;database=educhat;user=db;password=Tom7494");
        string connString = "server = 127.0.0.1; port=3306;database=educhat;user=db;password=Tom7494";




        public async Task<bool> IsUsernameFree(string username)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand($"SELECT * FROM user WHERE UserName='{username}' AND IsDeleted = 0", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync()) return false;
                    else return true;
            }
        }

        public async Task<bool> IsEmailFree(string email)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand($"SELECT * FROM user WHERE UserEmail='{email}' AND IsDeleted = 0;", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync()) return false;
                    else return true;
            }
        }


        public async Task<User> GetUserById(int UserId, bool flatten = false)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand($"SELECT * FROM user WHERE `UserId`={UserId} AND IsDeleted = 0;", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            UserId = Convert.ToInt32(reader["UserId"]), UserEmail = reader["UserEmail"].ToString(), UserName = reader["UserName"].ToString(),
                            UserFullName = reader["UserFullName"].ToString(), UserGender = reader["UserGender"].ToString(), UserDOB = Convert.ToDateTime(reader["UserDOB"]),
                            UserProfilePictureURL = reader["UserProfilePictureURL"].ToString(), UserSchool = reader["UserSchool"].ToString(), IsModerator = Convert.ToBoolean(reader["IsModerator"]),
                            IsAdmin = Convert.ToBoolean(reader["IsAdmin"]), IsDeleted = Convert.ToBoolean(reader["IsDeleted"]), UserPassHash = reader["UserPassHash"].ToString(),
                            Subjects = !flatten ? await new SubjectTasks().GetSubscribedSubjects(UserId) : null,
                            Chats = !flatten ? await new ChatTasks().GetAllChatsForUser(UserId) : null
                        };
                    }
                    else return null;
            }
        }

        public async Task<User> AuthenticateUser(AuthenticatingUser usr)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand($"SELECT UserId FROM user WHERE (UserName='{usr.Authenticator}' AND UserPassHash='{usr.PassHash}')" +
                $" OR (UserEmail='{usr.Authenticator}' AND UserPassHash='{usr.PassHash}');", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync()) return await GetUserById(Convert.ToInt32(reader["userId"]));
                    else return null;
                    
            }
        }
        
        public async Task<User> CreateNewUser(User usr)
        {
            using (var conn = new MySqlConnection(connString)) //Creates a connection that will be destroyed once scope ends
            {
                await conn.OpenAsync(); //Waits for the connection to open
                using (var cmd = new MySqlCommand()) //Creates comman that will be destroyed once scope ends
                {
                    cmd.Connection = conn; //Sets command's connection to our connection
                    cmd.CommandText = $"INSERT INTO user VALUES (0, '{usr.UserEmail}', '{usr.UserName}', '{usr.UserFullName}', '{usr.UserProfilePictureURL}', '{usr.UserSchool}', '{usr.UserGender}'," +
                    $"'{usr.UserDOB.ToString("yyyy-MM-dd hh:mm:ss")}', {usr.IsModerator}, {usr.IsAdmin}, {usr.IsDeleted}, '{usr.UserPassHash}');";
                    // ^ the SQL statement to insert the user
                    await cmd.ExecuteNonQueryAsync(); //Waits for the command to execute
                    int id = (int)cmd.LastInsertedId; //Gets the auto incremented Id from that last insertion
                    usr.UserId = id; return usr; //Sets the user object's id to the above, and returns the user
                }
            }
        }

        public async Task<User> UploadProfilePicture(IFormFile file, int UserId) //Takes an IFormFile and UserId
        {
            Directory.CreateDirectory($"/var/www/cdn/ProfilePics/{UserId}/"); //Creates the folder for the user under the cdn directory
            var filePath = $"/var/www/cdn/ProfilePics/{UserId}/{file.FileName}"; //Sets the file path, consisting of the directory above and the actual file name
            using (var stream = new FileStream(filePath, FileMode.Create)) //the using keyword means this FileStream will be destroyed once it has completed
            {
                await file.CopyToAsync(stream); //Creates the file at the filePath;
            }
            return await EditUserProfilePic(UserId, $"https://cdn.tomk.online/ProfilePics/{UserId}/{file.FileName}"); //Returns the new user with the url to the file we just created
        }

        public async Task<User> ModifyUser(int id, User usr)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = $"UPDATE user SET `UserEmail` = '{usr.UserEmail}', `UserName` = '{usr.UserName}', `UserFullName` = '{usr.UserFullName}'," +
                    $" `UserProfilePictureURL` = '{usr.UserProfilePictureURL}', `UserSchool` = '{usr.UserSchool}', `UserGender` = '{usr.UserGender}', `UserDOB` = '{usr.UserDOB.ToString("yyyy-MM-dd hh:mm:ss")}'," +
                    $" `IsModerator` = {usr.IsModerator}, `IsAdmin` = {usr.IsAdmin}, `IsDeleted` = {usr.IsDeleted}, `UserPassHash` = '{usr.UserPassHash}' WHERE (`UserId` = {id});";
                    await cmd.ExecuteNonQueryAsync();
                    return await GetUserById(id);
                }
            }
        }

        public async Task<User> EditUserProfilePic(int UserId, string url)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = $"UPDATE user SET UserProfilePictureURL='{url}' WHERE UserId={UserId};";
                    await cmd.ExecuteNonQueryAsync();
                    return await GetUserById(UserId);
                }
            }
        }




        public async Task<User>SubscripeUserToSubjects(int UserId, List<int> SubjectIds)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                foreach (int id in SubjectIds)
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = $"INSERT INTO subject_enrollment VALUES ({UserId}, {id}, 1) ON DUPLICATE KEY UPDATE UserId = VALUES(UserId), SubjectId = VALUES(SubjectId), IsEnabled = VALUES(IsEnabled);";
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return await GetUserById(UserId);
            }
        }

        public async Task<User>UnsubscribeUserToSubjects(int UserId, List<int> SubjectIds)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                foreach (int id in SubjectIds)
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = $"INSERT INTO subject_enrollment VALUES ({UserId}, {id}, 0) ON DUPLICATE KEY UPDATE UserId = VALUES(UserId), SubjectId = VALUES(SubjectId), IsEnabled = VALUES(IsEnabled);";
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return await GetUserById(UserId);
            }
        }
    }
}
