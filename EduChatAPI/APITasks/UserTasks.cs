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
                    IsAdmin = Convert.ToBoolean(reader["IsAdmin"]), IsDeleted = Convert.ToBoolean(reader["IsDeleted"]), UserPassHash = reader["UserPassHash"].ToString()
                }; //Returns the user object populated with the data from the mysql database
                conn.Close();
                return usr;
            }
            conn.Close();
            return null; //If the record does not exist, return null

        }

        public async Task<User> CreateNewUser(User usr)
        {
            //IsUsernameFree and IsEmailFree should be checked from the application first, UploadProfilePicture should have already run
            await conn.OpenAsync();
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO user VALUES (0, '{usr.UserEmail}', '{usr.UserName}', '{usr.UserFullName}', '{usr.UserProfilePictureURL}', '{usr.UserSchool}', '{usr.UserGender}'," +
                $"'{usr.UserDOB}', b'{usr.IsModerator}', b'{usr.IsAdmin}', b'{usr.IsDeleted}', '{usr.UserPassHash}');", conn); //Inserts the row into the table 
            // The letter b indicated the next value should be converted to a byte, this is for boolean values
            await cmd.ExecuteNonQueryAsync(); //awaits the execution of the above statement
            int id = (int)cmd.LastInsertedId; // The UserId is autoincremented, and we need to get the value it just inserted as an integer
            conn.Close(); //CLoses the connection
            usr.UserId = id; //We set the user's id to the last inserted one, as that is all we have changed
            return usr; //We return the original user with the new user id - it is now in the database
        }

        public async Task<string> UploadProfilePicture(IFormFile file, int UserId) //Takes an IFormFile and UserId
        {
            Directory.CreateDirectory($"/var/www/cdn/ProfilePics/{UserId}/"); //Creates the folder for the user under the cdn directory
            var filePath = $"/var/www/cdn/ProfilePics/{UserId}/{file.FileName}"; //Sets the file path, consisting of the directory above and the actual file name
            using (var stream = new FileStream(filePath, FileMode.Create)) //the using keyword means this FileStream will be destroyed once it has completed
            {
                await file.CopyToAsync(stream); //Creates the file at the filePath;
            }
            return $"https://cdn.tomk.online/ProfilePics/{UserId}/{file.FileName}"; //Returns the url to the file we just created
        }
    }
}
