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
        MySqlConnection conn = new MySqlConnection("server=127.0.0.1;port=3306;database=educhat;user=db;password=Tom7494");





        public async Task<bool> IsUsernameFree(string username)
        {
            await conn.OpenAsync(); //Opens the conenection to the database
            MySqlDataReader reader = new MySqlCommand($"SELECT * FROM user WHERE UserName='{username}' AND IsDeleted = 0", conn).ExecuteReader(); //Selects all where username matches the one we're checking
            if (reader.Read()) { conn.Close(); return false; } //If it finds any, return false and close the connection
            else { conn.Close(); return true; } //Else, close the connection and return true
        }

        public async Task<bool> IsEmailFree(string email)
        {
            await conn.OpenAsync(); //Opens the conenection to the database
            MySqlDataReader reader = new MySqlCommand($"SELECT * FROM user WHERE UserEmail='{email}' AND IsDeleted = 0 ", conn).ExecuteReader(); //Selects all where UserEmail matches the one we're checking
            if (reader.Read()) { conn.Close(); return false; } //If it finds any, return false and close the connection
            else { conn.Close(); return true; } //Else, close the connection and return true
        }


        public async Task<User> GetUserById(int UserId)
        {
            await conn.OpenAsync(); //Opens the connection the the mysql database 
            MySqlDataReader reader = new MySqlCommand($"SELECT * FROM user WHERE `UserId`={UserId}", conn).ExecuteReader(); //Executes the select command, looking for a record with a matching UserID
            if (reader.Read() && !Convert.ToBoolean(reader["IsDeleted"])) //If a record exists
            {        
                User usr=  new User
                {
                    UserId = Convert.ToInt32(reader["UserId"]), UserEmail = reader["UserEmail"].ToString(), UserName = reader["UserName"].ToString(),
                    UserFullName = reader["UserFullName"].ToString(), UserGender = reader["UserGender"].ToString(), UserDOB = Convert.ToDateTime(reader["UserDOB"]),
                    UserProfilePictureURL = reader["UserProfilePictureURL"].ToString(), UserSchool = reader["UserSchool"].ToString(), IsModerator = Convert.ToBoolean(reader["IsModerator"]),
                    IsAdmin = Convert.ToBoolean(reader["IsAdmin"]), IsDeleted = Convert.ToBoolean(reader["IsDeleted"]), UserPassHash = reader["UserPassHash"].ToString(),
                    Subjects = await new SubjectTasks().GetSubscribedSubjects(UserId)
                }; //Returns the user object populated with the data from the mysql database
                conn.Close();
                return usr;
            }
            conn.Close();
            return null; //If the record does not exist, return null

        }

        public async Task<User> AuthenticateUser(AuthenticatingUser usr)
        {
            await conn.OpenAsync();
            MySqlDataReader reader = new MySqlCommand($"SELECT UserId FROM user WHERE (UserName='{usr.Authenticator}' AND UserPassHash='{usr.PassHash}')" +
                $" OR (UserEmail='{usr.Authenticator}' AND UserPassHash='{usr.PassHash}');", conn).ExecuteReader(); //The select command gets the UserId from the row where either the email and password match,
            //or the username and password match
           
            if (reader.Read())
            {
                int id = Convert.ToInt32(reader["UserId"]);
                Debug.WriteLine(id);
                conn.Close();
                return await GetUserById(id);
            } //Gets the user from the ID returned
            else { conn.Close(); return null; } //returns null if not found

        }

        public async Task<User> CreateNewUser(User usr)
        {
            //IsUsernameFree and IsEmailFree should be checked from the application first, UploadProfilePicture should have already run
            await conn.OpenAsync();
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO user VALUES (0, '{usr.UserEmail}', '{usr.UserName}', '{usr.UserFullName}', '{usr.UserProfilePictureURL}', '{usr.UserSchool}', '{usr.UserGender}'," +
                $"'{usr.UserDOB.ToString("yyyy-MM-dd hh:mm:ss")}', {usr.IsModerator}, {usr.IsAdmin}, {usr.IsDeleted}, '{usr.UserPassHash}');", conn); //Inserts the row into the table 
            await cmd.ExecuteNonQueryAsync(); //awaits the execution of the above statement
            int id = (int)cmd.LastInsertedId; // The UserId is autoincremented, and we need to get the value it just inserted as an integer
            conn.Close(); //CLoses the connection
            usr.UserId = id; //We set the user's id to the last inserted one, as that is all we have changed
            return usr; //We return the original user with the new user id - it is now in the database
        }

        public async Task<User> UploadProfilePicture(IFormFile file, int UserId) //Takes an IFormFile and UserId
        {
            Directory.CreateDirectory($"/var/www/cdn/ProfilePics/{UserId}/"); //Creates the folder for the user under the cdn directory
            var filePath = $"/var/www/cdn/ProfilePics/{UserId}/{file.FileName}"; //Sets the file path, consisting of the directory above and the actual file name
            using (var stream = new FileStream(filePath, FileMode.Create)) //the using keyword means this FileStream will be destroyed once it has completed
            {
                await file.CopyToAsync(stream); //Creates the file at the filePath;
            }
            conn.Close();
            return await EditUserProfilePic(UserId, $"https://cdn.tomk.online/ProfilePics/{UserId}/{file.FileName}"); //Returns the new user with the url to the file we just created
        }

        public async Task<User> ModifyUser(int id, User usr)
        {
            await conn.OpenAsync();
            MySqlCommand cmd = new MySqlCommand($"UPDATE user SET `UserEmail` = '{usr.UserEmail}', `UserName` = '{usr.UserName}', `UserFullName` = '{usr.UserFullName}'," +
                $" `UserProfilePictureURL` = '{usr.UserProfilePictureURL}', `UserSchool` = '{usr.UserSchool}', `UserGender` = '{usr.UserGender}', `UserDOB` = '{usr.UserDOB.ToString("yyyy-MM-dd hh:mm:ss")}'," +
                $" `IsModerator` = {usr.IsModerator}, `IsAdmin` = {usr.IsAdmin}, `IsDeleted` = {usr.IsDeleted}, `UserPassHash` = '{usr.UserPassHash}' WHERE (`UserId` = {id})", conn);
            await cmd.ExecuteNonQueryAsync();
            conn.Close();
            return await GetUserById(id);
        }

        public async Task<User> EditUserProfilePic(int UserId, string url)
        {
            await conn.OpenAsync();
            MySqlCommand cmd = new MySqlCommand($"UPDATE user SET UserProfilePictureURL='{url}' WHERE UserId={UserId};", conn);
            await cmd.ExecuteNonQueryAsync();
            conn.Close();
            return await GetUserById(UserId);
        }




        public async Task<User>SubscripeUserToSubjects(int UserId, List<int> SubjectIds)
        {
            await conn.OpenAsync();
            foreach (int id in SubjectIds)
            {
                MySqlCommand cmd = new MySqlCommand($"INSERT INTO subject_enrollment VALUES ({UserId}, {id}, 1) ON DUPLICATE KEY UPDATE UserId = VALUES(UserId), SubjectId = VALUES(SubjectId), IsEnabled = VALUES(IsEnabled);", conn);
                cmd.ExecuteNonQuery();
            }
            conn.Close();
            return await GetUserById(UserId);
        }

        public async Task<User>UnsubscribeUserToSubjects(int UserId, List<int> SubjectIds)
        {
            await conn.OpenAsync();
            foreach (int id in SubjectIds)
            {
                MySqlCommand cmd = new MySqlCommand($"INSERT INTO subject_enrollment VALUES ({UserId}, {id}, 0) ON DUPLICATE KEY UPDATE UserId = VALUES(UserId), SubjectId = VALUES(SubjectId), IsEnabled = VALUES(IsEnabled);", conn);
                cmd.ExecuteNonQuery();
            }
            conn.Close();
            return await GetUserById(UserId);
        }
    }
}
