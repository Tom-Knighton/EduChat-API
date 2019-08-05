using EduChatAPI.Objects;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EduChatAPI.APITasks
{
    public class UserTasks
    {
        MySqlConnection conn = new MySqlConnection("server=127.0.0.1;port=3306;database=educhat;user=db;password=Tom7494");



        public async Task<User> GetUserById(int UserId)
        {
            await conn.OpenAsync(); //Opens the connection the the mysql database 
            MySqlDataReader reader = new MySqlCommand($"SELECT * FROM user WHERE `UserId`={UserId}", conn).ExecuteReader(); //Executes the select command, looking for a record with a matching UserID
            if (reader.Read()) //If a record exists
            {
                Debug.WriteLine(reader["UserId"].ToString());
                Debug.WriteLine(reader.ToString());
                return new User
                {
                    UserId = Convert.ToInt32(reader["UserId"]), UserEmail = reader["UserEmail"].ToString(), UserName = reader["UserName"].ToString(),
                    UserFullName = reader["UserFullName"].ToString(), UserGender = reader["UserGender"].ToString(), UserDOB = Convert.ToDateTime(reader["UserDOB"]),
                    UserProfilePictureURL = reader["UserProfilePictureURL"].ToString(), UserSchool = reader["UserSchool"].ToString(), UserPassHash = reader["UserPassHash"].ToString()
                }; //Returns the user object populated with the data from the mysql database
            }
            return null; //If the record does not exist, return null

        }
    }
}
