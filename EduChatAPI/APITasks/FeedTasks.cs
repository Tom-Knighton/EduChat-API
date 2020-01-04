using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EduChatAPI.Objects;
using EduChatAPI.Objects.Feed;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace EduChatAPI.APITasks
{
    public class FeedTasks
    {
        string connString = "server=127.0.0.1;port=3306;database=educhat;user=db;password=Tom7494";

        public async Task<FeedPost> GetPostById(int postid) {
            using (var conn = new MySqlConnection(connString)) //Creates a temporary new Connectio
            {
                await conn.OpenAsync(); //Waits for connection to open
                using (var cmd = new MySqlCommand($"SELECT * FROM feed_post WHERE `PostId`={postid};", conn)) //Select command to get the row of the id
                using (var reader = await cmd.ExecuteReaderAsync()) //Executes the above command
                {
                    if (await reader.ReadAsync()) //If any row was returned
                    {
                        FeedPost post = new FeedPost //Create new FeedPost object from our returned values
                        {
                            postId = Convert.ToInt32(reader["postId"]), posterId = Convert.ToInt32(reader["posterId"]),
                            postType = reader["postType"].ToString(), subjectId = Convert.ToInt32(reader["subjectId"]),
                            datePosted = Convert.ToDateTime(reader["datePosted"]), isAnnouncement = Convert.ToBoolean(reader["isAnnouncement"]),
                            isDeleted = Convert.ToBoolean(reader["isDeleted"]), poster = await new UserTasks().GetUserById(Convert.ToInt32(reader["posterId"]), flatten: true)
                        };
                        string json = Json.Stringify(post); //Convert the above object into a json string
                        switch (post.postType) //What to do for each post type
                        {
                            case "text": //IF it is a text post
                                FeedTextPost tPost = Json.Parse<FeedTextPost>(json); //Convert the above json into a FeedTextPost object
                                tPost = await AddTextPostValues(tPost); //Create a new object from the above one, with our additional text values
                                return tPost; //return it
                            case "media":
                                 FeedMediaPost mPost = Json.Parse<FeedMediaPost>(json); //Convert the abve json intoa FeedMediaPost object
                                 mPost = await AddMediaPostValues(mPost); //Create a new object from the above, with our additional media values
                                 return mPost; //return it
                            default: return null; //If the switch statement fails, return nothing.
                        }
                    }
                }
                return null; //If no row is returned, return nothing.
            }
        }

        public async Task<List<FeedPost>> GetAllPostsForSubject(int subjid)
        {
            List<FeedPost> posts = new List<FeedPost>();
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand($"SELECT * FROM feed_post WHERE `SubjectId`={subjid} AND IsDeleted={false};", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        posts.Add(await GetPostById(Convert.ToInt32(reader["postId"])));
                    }
                }
            }
            posts.Sort((x, y) => y.datePosted.CompareTo(x.datePosted));
            return posts;
        }

        public async Task<FeedTextPost> AddTextPostValues(FeedTextPost post)
        {
            using (var conn = new MySqlConnection(connString)) //New connection
            { 
                await conn.OpenAsync(); //Open connection
                using (var cmd = new MySqlCommand($"SELECT * FROM feed_text_post WHERE `PostId`={post.postId};", conn))
                // ^ Select all values from the selected row
                using (var reader = await cmd.ExecuteReaderAsync()) //Read the row
                { 
                    if (await reader.ReadAsync()) //IF anything is returned
                    {
                        post.postText = reader["postText"].ToString(); //Add the postText value to the object
                        return post; //return the object
                    }
                }
                return null; //else, return null.

            } 
        }
        public async Task<FeedMediaPost> AddMediaPostValues(FeedMediaPost post)
        {
            using (var conn = new MySqlConnection(connString)) //New connection
            { 
                await conn.OpenAsync(); //Opens connection
                using (var cmd = new MySqlCommand($"SELECT * FROM feed_media_post WHERE `PostId`={post.postId};", conn))
                // ^ Selects all values frm the selecte drow
                using (var reader = await cmd.ExecuteReaderAsync()) //Read the row
                {
                    if (await reader.ReadAsync()) //If anything is returned
                    {
                        post.urlToPost = reader["urlToPost"].ToString(); post.isVideo = Convert.ToBoolean(reader["isVideo"]);
                        post.postDescription = reader["description"].ToString();
                        // ^ Add the media values to the original object
                        return post; //return the object
                    } 
                }
                return null; //Else, return nothing.
            }
        }
    }
}
