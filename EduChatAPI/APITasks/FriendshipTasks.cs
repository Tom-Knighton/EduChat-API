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
    public class FriendshipTasks
    {
        // MySqlConnection conn = new MySqlConnection("server=127.0.0.1;port=3306;database=educhat;user=db;password=Tom7494");
        string connString = "server = 127.0.0.1; port=3306;database=educhat;user=db;password=Tom7494";

        public async Task<Friendship> GetFriendship(int userid1, int userid2)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand($"SELECT * FROM user_friendships WHERE FirstUserId='{userid1}' AND SecondUserId='{userid2}';", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync()) return new Friendship
                    {
                        FirstUserId = userid1, SecondUserId = userid2, IsBestFriend = Convert.ToBoolean(reader["IsBestFriend"]), IsBlocked = Convert.ToBoolean(reader["IsBlocked"])
                    };
                return null;
            }
        }

        public async Task<bool> DoesFriendshipExist(int userId1, int userId2)
        {   
            using (var conn = new MySqlConnection(connString)) //Creates connection that deletes itself once scope is over
            {
                await conn.OpenAsync(); //waits for conn to open
                using (var cmd = new MySqlCommand($"SELECT * FROM user_friendships WHERE (FirstUserId='{userId1}' AND SecondUserId='{userId2}') OR (FirstUserId='{userId2}' AND SecondUserId='{userId1}');", conn))
                // ^ selects the row where a friendship exists between the two users
                using (var reader = await cmd.ExecuteReaderAsync()) //Execute the command
                    if (await reader.ReadAsync()) return true; //If a line exists return true
                return false; //else return false

            }
        }

        public async Task<bool> IsBlocked(int userId1, int userId2)
        {
            using (var conn = new MySqlConnection(connString)) //Creates connection that deletes itself once scope is over
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand($"SELECT * FROM user_friendships WHERE FirstUserId='{userId1}' AND SecondUserId='{userId2}' AND IsBlocked={true};", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync()) return true;
                return false;
            }
        }

        public async Task<Friendship> SetBlock(int userid1, int userid2, bool blocked)
        {
            using (var conn = new MySqlConnection(connString)) //Creates connection that deletes itself once scope is over
            {
                await conn.OpenAsync(); //waits for conn to open
                Friendship current = await GetFriendship(userid1, userid2); //Gets the current friendship if one exists
                using (var cmd = new MySqlCommand($"INSERT INTO user_friendships VALUES({userid1}, {userid2}, {blocked}, {(current != null ? current.IsBestFriend : false)})" +
                    $" ON DUPLICATE KEY UPDATE IsBlocked=VALUES(IsBlocked);", conn)) //Inserts a new row into the table setting the friendship IsBlocked to true. If a friendship already exists
                    // then then a new row is not inserted and the IsBlocked value is just updated
                await cmd.ExecuteNonQueryAsync(); //Executes the command 
                return await GetFriendship(userid1, userid2); //Returns the new friendship object
            }
        }

        public async Task<Friendship> CreateFriendship(Friendship friendship)
        {
            using (var conn = new MySqlConnection(connString)) //Creates connection that deletes itself once scope is over
            {
                await conn.OpenAsync(); //waits for conn to open               
                using (var cmd = new MySqlCommand($"INSERT INTO user_friendships VALUES({friendship.FirstUserId}, {friendship.SecondUserId}, {friendship.IsBlocked}," +
                    $" {friendship.IsBestFriend}" +
                    $" ON DUPLICATE KEY UPDATE IsBlocked=VALUES(IsBlocked), IsBestFriend=VALUES(IsBestFriend);", conn)) //Inserts a new row into the table setting the friendship IsBlocked to true. If a friendship already exists
                                                                                     // then then a new row is not inserted and the IsBlocked value is just updated
                    await cmd.ExecuteNonQueryAsync(); //Executes the command 
                return friendship; //Returns the new friendship object
            }
        }

        public async Task<List<Friendship>> GetAllFriendsForUser(int userid)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                List<Friendship> friendships = new List<Friendship>();
                using (var cmd = new MySqlCommand($"SELECT * FROM user_friendships WHERE FirstUserId='{userid}';", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync())
                    {
                        friendships.Add(new Friendship
                        {
                            FirstUserId = userid, SecondUserId = Convert.ToInt32(reader["SecondUserId"]), IsBestFriend = Convert.ToBoolean(reader["IsBestFriend"]),
                            IsBlocked = Convert.ToBoolean(reader["IsBlocked"]), User2 = await new UserTasks().GetUserById(Convert.ToInt32(reader["SecondUserId"]), flatten: true)

                        });
                    }
                return friendships;
            }
        }
    }
}
