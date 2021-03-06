﻿using System;
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
                            case "poll": //If it is a poll
                                FeedPoll pPost = Json.Parse<FeedPoll>(json); //Convert above json to a FeedPoll object
                                pPost = await AddPollPostValues(pPost); //Create a new object from the above, including additional values
                                return pPost; //return it
                            case "quiz":
                                FeedQuiz fPost = Json.Parse<FeedQuiz>(json); //Convert json into FeedQuiz object
                                fPost = await AddBasicQuizValues(fPost); //Adds quiz values
                                return fPost; //return it
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

        public async Task<List<FeedPost>> GetAllPostsForUser(int userid)
        {
            List<FeedPost> posts = new List<FeedPost>(); //Initialises list of FeedPosts
            using (var conn = new MySqlConnection(connString)) //New connection
            {
                await conn.OpenAsync(); //Waits for connection to open
                using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM feed_post WHERE `PosterId`={userid} AND IsDeleted={false};", conn))
                // Selects all not deleted posts from the user selected
                using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync()) //For each row
                {
                    posts.Add(await GetPostById(Convert.ToInt32(reader["postId"]))); //Get Post object
                }
            }
            posts.Sort((x, y) => y.datePosted.CompareTo(x.datePosted)); //Sort by date posted
            return posts; //return list
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

        public async Task<FeedQuiz> AddBasicQuizValues(FeedQuiz quiz)
        {
            using (var conn = new MySqlConnection(connString)) //New connection
            {
                await conn.OpenAsync(); //Waits to open
                using (var cmd = new MySqlCommand($"SELECT * FROM feed_quiz_post WHERE `PostId`={quiz.postId};", conn))
                //^ Selects all data from feed_quiz for our post
                using (var reader = await cmd.ExecuteReaderAsync()) //reads the data
                {
                    if (await reader.ReadAsync()) //IF we found anything
                    {
                        quiz.QuizTitle = reader["postTitle"].ToString();
                        quiz.DifficultyLevel = reader["overallDifficulty"].ToString();
                        return quiz;
                    }
                }
                return null; //else return nothing
            }
        }

        public async Task<FeedPoll> AddPollPostValues(FeedPoll post)
        {
            using (var conn = new MySqlConnection(connString)) //New connection
            {
                await conn.OpenAsync(); //Waits to open
                using (var cmd = new MySqlCommand($"SELECT * FROM feed_poll WHERE `PostId`={post.postId};", conn))
                    //^ Selects all data from feed_poll for our post
                using (var reader = await cmd.ExecuteReaderAsync()) //reads the data
                {
                    if (await reader.ReadAsync()) //IF we found anything
                    {
                        post.PollQuestion = reader["pollQuestion"].ToString();
                        post.Answers = await AddPollAnswers(post.postId);
                        return post; //Add the poll attributes to the post object
                    }
                }
                return null; //else return nothing
            }
        }
        public async Task<List<FeedAnswer>> AddPollAnswers(int postId)
        {
            using (var conn = new MySqlConnection(connString)) //New connection
            {
                List<FeedAnswer> answers = new List<FeedAnswer>(); //Empty list
                await conn.OpenAsync(); //Opens connection
                using (var cmd = new MySqlCommand($"SELECT * FROM feed_poll_answer WHERE `PostId`={postId};", conn))
                    //^ Selects all answers from the table
                using (var reader = await cmd.ExecuteReaderAsync()) //Reads data
                {
                    while (await reader.ReadAsync()) //For each row returned
                    {
                        answers.Add(new FeedAnswer //Add a new FeedAnswer object to our list
                        {
                            AnswerId = Convert.ToInt32(reader["answerId"]), PostId = postId, Answer = reader["answer"].ToString(),
                            Votes = await AddPollVotes(Convert.ToInt32(reader["answerId"]))
                        });
                    }
                }
                return answers; //return the list
            }
        }
        public async Task<List<FeedAnswerVote>> AddPollVotes(int answerId)
        {
            using (var conn = new MySqlConnection(connString)) //New connection
            {
                List<FeedAnswerVote> votes = new List<FeedAnswerVote>(); //Empty list
                await conn.OpenAsync(); //waits to open
                using (var cmd = new MySqlCommand($"SELECT * FROM feed_poll_vote WHERE `AnswerId`={answerId} AND IsDeleted={false};", conn))
                    //^ Selects all non deleted votes for each answer
                using (var reader = await cmd.ExecuteReaderAsync()) //Reads data
                {
                    while (await reader.ReadAsync()) //FOr each row
                    {
                        votes.Add(new FeedAnswerVote //Add a new FeedAnswerVote object to list
                        {
                            AnswerId = answerId, IsDeleted = false, UserId = Convert.ToInt32(reader["userId"])
                        });
                    }
                }
                return votes; //return the list
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
         


        //POLL:
        public async Task<List<FeedAnswer>> VoteForPoll(int userid, int answerid, int pollid)
        {
            using (var conn = new MySqlConnection(connString)) //New connection
            {
                await conn.OpenAsync(); //waits for connection to open
                using (var cmd = new MySqlCommand($"INSERT INTO feed_poll_vote VALUES({answerid}, {userid}, {false}) ON DUPLICATE KEY UPDATE IsDeleted={false};", conn))
                    //^ inserts vote into row, if it already exists, just set isdeleted to false
                    await cmd.ExecuteNonQueryAsync(); //Execute command
                return await AddPollAnswers(pollid); //Return list of answers with votes for poll
            }
        }

        public async Task<FeedPost> UploadPollPost(FeedPoll poll)
        {
            using (var conn = new MySqlConnection(connString)) //New connection
            {
                await conn.OpenAsync(); //Waits for connection to open
                using (var cmd = new MySqlCommand($"INSERT INTO feed_post VALUES({0}, 'poll', {poll.posterId}, {poll.subjectId}, '{poll.datePosted.ToString("yyyy-MM-dd hh:mm:ss")}', " +
                    $"{Convert.ToBoolean(poll.isAnnouncement)}, {Convert.ToBoolean(poll.isDeleted)});", conn)) //Inserts post into feed_post
                {
                    await cmd.ExecuteNonQueryAsync(); //Executes that command
                    int id = (int)cmd.LastInsertedId; //Gets the last inserted auto incremented id
                    using (var cmd2 = new MySqlCommand($"INSERT INTO feed_poll VALUES({id}, '{poll.PollQuestion}');", conn))
                        await cmd2.ExecuteNonQueryAsync(); //Inserts value into feed_poll values
                    string answersCommand = ""; //Sets up an empty string
                    foreach (FeedAnswer answer in poll.Answers) //For each answer in the answers array
                    { // Add to the answersCommand string, insert each feed_poll_answer row
                        answersCommand += $" INSERT INTO feed_poll_answer VALUES({0}, {id}, '{answer.Answer}');";
                    }
                    using (var cmd3 = new MySqlCommand(answersCommand, conn)) //Executes all the commands above
                        await cmd3.ExecuteNonQueryAsync();
                    return await GetPostById(id); //Returns the FeedPoll object
                }
            }
        }

        //QUIZ:
        public async Task<FeedQuiz> GetFullFeedQuiz(int QuizId)
        {
            FeedQuiz quiz = (FeedQuiz) await GetPostById(QuizId); //Gets base FeedPost obkect
            List<FeedQuizQuestion> questions = new List<FeedQuizQuestion>();
            List<FeedQuizResult> results = new List<FeedQuizResult>();
            using (var conn = new MySqlConnection(connString)) //New connection
            {
                await conn.OpenAsync(); //Waits for connection to open
                using (var questionsCmd = new MySqlCommand($"SELECT * FROM feed_quiz_question WHERE PostId={QuizId};", conn))
                    // ^ selects all rows in the question table for this quiz 
                using (var qReader = await questionsCmd.ExecuteReaderAsync()) //reads the data
                    while (await qReader.ReadAsync()) //For each row returndd
                    {
                        questions.Add(new FeedQuizQuestion //Add a new Question object to the array
                        {
                            PostId = QuizId, QuestionId = Convert.ToInt32(qReader["questionId"]),
                            CorrectAnswer = qReader["correctAnswer"].ToString(),
                            Answers = new List<string> { qReader["answer1"].ToString(), qReader["answer2"].ToString(),
                               qReader["answer3"].ToString(), qReader["answer4"].ToString() },
                            Difficulty = Convert.ToInt32(qReader["questionDifficulty"]),
                            Question = qReader["question"].ToString()
                        });

                    }
                using (var resultsCmd = new MySqlCommand($"SELECT * FROM feed_quiz_result WHERE PostId={QuizId};", conn))
                // ^ Selects all rows in the result table for this quiz
                using (var rReader = await resultsCmd.ExecuteReaderAsync())
                { //Reads the data
                    while (await rReader.ReadAsync())
                    { //For each row returned
                        results.Add(new FeedQuizResult //Adds a new Result object to the array
                        {
                            PostId = QuizId,
                            UserId = Convert.ToInt32(rReader["userId"]),
                            OverallScore = Convert.ToInt32(rReader["overallScore"]),
                            User = await new UserTasks().GetUserById(Convert.ToInt32(rReader["userId"]), flatten: true),
                            DatePosted = Convert.ToDateTime(rReader["datePosted"])
                        });
                    }
                    quiz.Questions = questions; quiz.Results = results; //Adds questions and results
                    return quiz; //Returns modified quiz object
                }
            }
        }

        public async Task<FeedQuizResult> GetQuizResultById(int id)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand($"SELECT * FROM feed_quiz_result WHERE ResultId={id};", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync())
                    {
                        return new FeedQuizResult
                        {
                            PostId = Convert.ToInt32(reader["postId"]), UserId = Convert.ToInt32(reader["userId"]),
                            OverallScore = Convert.ToInt32(reader["overallScore"]), DatePosted = Convert.ToDateTime(reader["datePosted"]),
                            User = await new UserTasks().GetUserById(Convert.ToInt32(reader["userId"]), flatten: true)
                        };
                    }
                return null;


            }
        }

        public async Task<FeedQuizResult> CreateQuizResult(FeedQuizResult result, int QuizId)
        {
            using (var conn = new MySqlConnection(connString)) //New temporary connection
            { 
                await conn.OpenAsync(); //Opens connection
                using (var cmd = new MySqlCommand($"INSERT INTO feed_quiz_result VALUES({0}, {QuizId}, {result.UserId}, {result.OverallScore}, '{result.DatePosted.ToString("yyyy-MM-dd hh:mm:ss")}');", conn))
                { //^ inserts values into feed_quiz_result row
                    await cmd.ExecuteNonQueryAsync(); //Executes command
                    return await GetQuizResultById((int)cmd.LastInsertedId); //Get returned reuslt row
                }
            }
        }

        public async Task<FeedPost> UploadQuizPost(FeedQuiz quiz)
        {
            using (var conn = new MySqlConnection(connString)) //New connection
            {
                await conn.OpenAsync(); //Waits for connection to open
                using (var cmd = new MySqlCommand($"INSERT INTO feed_post VALUES({0}, 'quiz', {quiz.posterId}, {quiz.subjectId}, '{quiz.datePosted.ToString("yyyy-MM-dd hh:mm:ss")}', " +
                    $"{Convert.ToBoolean(quiz.isAnnouncement)}, {Convert.ToBoolean(quiz.isDeleted)});", conn)) //Inserts post into feed_post
                {
                    await cmd.ExecuteNonQueryAsync(); //Executes that command
                    int id = (int)cmd.LastInsertedId; //Gets the last inserted auto incremented id
                    using (var cmd2 = new MySqlCommand($"INSERT INTO feed_quiz_post VALUES({id}, '{quiz.QuizTitle}', '{quiz.DifficultyLevel}');", conn))
                        await cmd2.ExecuteNonQueryAsync(); //Inserts value into feed_quiz_post values
                    string questionsCommand = ""; //Sets up an empty string
                    foreach (FeedQuizQuestion question in quiz.Questions) //For each question in the questions array
                    { // Add to the questionsCommand string, insert each feed_quiz_question row
                        questionsCommand += $" INSERT INTO feed_quiz_question VALUES({0}, {id}, '{question.Question}', " +
                            $"'{question.Answers[0]}', '{question.Answers[1]}', '{question.Answers[2]}', '{question.Answers[3]}', '{question.CorrectAnswer}', {question.Difficulty});";
                    }
                    using (var cmd3 = new MySqlCommand(questionsCommand, conn)) //Executes all the commands above
                        await cmd3.ExecuteNonQueryAsync();
                    return await GetPostById(id); //Returns the FeedQuiz object
                }
            }
        }
       
    }
}
