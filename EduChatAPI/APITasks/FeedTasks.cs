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
                            isDeleted = Convert.ToBoolean(reader["isDeleted"]), poster = await new UserTasks().GetUserById(Convert.ToInt32(reader["posterId"]), flatten: true),
                            likes = await GetAllLikesForPost(Convert.ToInt32(reader["postId"]))
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

        public async Task<List<FeedLike>> GetAllLikesForPost(int PostId)
        {
            using (var conn = new MySqlConnection(connString)) //Creates temp connection
            {
                List<FeedLike> likes = new List<FeedLike>(); //List of FeedLike objects
                await conn.OpenAsync(); //Opens connection
                using (var cmd = new MySqlCommand($"SELECT * FROM feed_post_likes WHERE `PostId`={PostId} AND `IsLiked`={true};", conn))
                    // ^ Gets all the rows for the post where IsLiked is true
                using (var reader = await cmd.ExecuteReaderAsync()) //Executes the command
                    while (await reader.ReadAsync()) //While we read each row
                    {
                        likes.Add(new FeedLike //Adds the new like object
                        {
                            PostId = Convert.ToInt32(reader["postId"]),
                            UserId = Convert.ToInt32(reader["userId"]),
                            IsLiked = Convert.ToBoolean(reader["isLiked"])
                        });
                    }
                return likes; //returns the list of likes
            }
        }

        public async Task<FeedPost> SetLikeStatus(int PostId, int UserId, bool setLiked)
        {
            using (var conn = new MySqlConnection(connString)) //Creates new temporary connection
            {
                await conn.OpenAsync(); //Opens the connection
                using (var cmd = new MySqlCommand($"INSERT INTO feed_post_likes VALUES({PostId}, {UserId}, {setLiked}) ON DUPLICATE KEY UPDATE IsLiked={setLiked};", conn))
                    await cmd.ExecuteNonQueryAsync(); //Inserts new like into the database, if a row for that user and post already exist, update the row
                return await GetPostById(PostId); //Return the new post object
            }
        }

        public async Task<List<FeedComment>> GetAllCommentsForPost(int PostId)
        {
            using (var conn = new MySqlConnection(connString)) //Creates temporary connection
            {
                List<FeedComment> comments = new List<FeedComment>(); //Empty list of FeedCommen objkects
                await conn.OpenAsync(); //Waits for the connection to open.
                using (var cmd = new MySqlCommand($"SELECT * FROM feed_post_comments WHERE PostId={PostId} AND IsDeleted={false};", conn))
                    //Selects all FeedComments for a specified post, that aren't deleted
                using (var reader = await cmd.ExecuteReaderAsync()) //Wait for the command to run
                    while (await reader.ReadAsync()) //For each row retuerned
                        comments.Add(new FeedComment //Add the new FeedComment object to the list
                        {
                            UserId = Convert.ToInt32(reader["userId"]), PostId = Convert.ToInt32(reader["postId"]),
                            CommentId = Convert.ToInt32(reader["commentId"]), IsAdmin = Convert.ToBoolean(reader["isAdmin"]),
                            IsDeleted = Convert.ToBoolean(reader["isDeleted"]), Comment = reader["comment"].ToString(),
                            user = await new UserTasks().GetUserById(Convert.ToInt32(reader["userId"]), flatten: true),
                            DatePosted = Convert.ToDateTime(reader["dateCommented"])
                            
                        });
                return comments; //return the list of FeedComment objects
            }
        }

        public async Task<FeedComment> GetCommentById(int CommentId)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand($"SELECT * FROM feed_post_comments WHERE CommentId={CommentId} AND IsDeleted={false};", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync())
                        return new FeedComment
                        {
                            UserId = Convert.ToInt32(reader["userId"]), PostId = Convert.ToInt32(reader["postId"]), CommentId = Convert.ToInt32(reader["commentId"]),
                            IsAdmin = Convert.ToBoolean(reader["isAdmin"]), IsDeleted = Convert.ToBoolean(reader["isDeleted"]), Comment = reader["comment"].ToString(),
                            user = await new UserTasks().GetUserById(Convert.ToInt32(reader["userId"]), flatten: true), DatePosted = Convert.ToDateTime(reader["dateCommented"])
                        };
                return null;
            }
        }

        public async Task<FeedComment> CreateNewCommentForPost(int PostId, FeedComment comment, int userId)
        {
            using (var conn = new MySqlConnection(connString)) //Creates new temp connection
            {
                await conn.OpenAsync(); //Waits for connection to open
                using (var cmd = new MySqlCommand($"INSERT INTO feed_post_comments VALUES({0}," + //Inserts new row into the table
                    $" {userId}, {PostId}, '{comment.Comment}', {comment.IsAdmin}, {comment.IsDeleted}, '{comment.DatePosted.ToString("yyyy-MM-dd hh:mm:ss")}');", conn))
                    //^ Inserts comment into table
                {
                    await cmd.ExecuteNonQueryAsync(); //Executes the command
                    return await GetCommentById((int)cmd.LastInsertedId); //Gets the last inserted auto-increment id and gets/returns the comment
                }
            }
        } 

        public async Task<FeedPost> UploadTextPost(FeedTextPost post)
        {
            using (var conn = new MySqlConnection(connString)) //Creates new temp connection
            {
                await conn.OpenAsync(); //Waits for connection to open
                using (var cmd = new MySqlCommand($"INSERT INTO feed_post VALUES({0}, 'text', {post.posterId}, {post.subjectId}, '{post.datePosted.ToString("yyyy-MM-dd hh:mm:ss")}', " +
                    $"{Convert.ToBoolean(post.isAnnouncement)}, {Convert.ToBoolean(post.isDeleted)});", conn)) //Inserts post into feed_post
                {
                    await cmd.ExecuteNonQueryAsync(); //Executes that command
                    using (var cmd2 = new MySqlCommand($"INSERT INTO feed_text_post VALUES ({cmd.LastInsertedId}, '{post.postText}');", conn)) //Inserts the above post into feed_text_post
                    {
                        await cmd2.ExecuteNonQueryAsync(); //Executes the above command
                        return await GetPostById((int)cmd.LastInsertedId);//Returns the new post object
                    }
                }
               
            }
        }
        public async Task<FeedPost> UploadMediaPost(FeedMediaPost post)
        {
            using (var conn = new MySqlConnection(connString)) //Creates new temp connection
            {
                await conn.OpenAsync(); //Waits for connection to open
                using (var cmd = new MySqlCommand($"INSERT INTO feed_post VALUES({0}, 'media', {post.posterId}, {post.subjectId}, '{post.datePosted.ToString("yyyy-MM-dd hh:mm:ss")}', " +
                    $"{Convert.ToBoolean(post.isAnnouncement)}, {Convert.ToBoolean(post.isDeleted)});", conn)) //Inserts post into feed_post
                {
                    await cmd.ExecuteNonQueryAsync(); //Executes that command
                    using (var cmd2 = new MySqlCommand($"INSERT INTO feed_media_post VALUES ({cmd.LastInsertedId}, '{post.urlToPost}', {post.isVideo}," +
                        $" '{post.postDescription}');", conn)) //Inserts the above post into feed_text_post
                    {
                        await cmd2.ExecuteNonQueryAsync(); //Executes the above command
                        return await GetPostById((int)cmd.LastInsertedId);//Returns the new post object
                    }
                }

            }
        }

        public async Task<string> UploadFeedMediaAttachment(IFormFile file)
        {
            string uuid = Guid.NewGuid().ToString();
            Directory.CreateDirectory("/var/www/cdn/FeedAttachments/Media/");
            string newFileName = file.FileName.Replace(Path.GetFileNameWithoutExtension(file.FileName), uuid);
            var filePath = $"/var/www/cdn/FeedAttachments/Media/{newFileName}";
            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);
            return $"https://cdn.tomk.online/FeedAttachments/Media/{newFileName}";
        }
    }
}
