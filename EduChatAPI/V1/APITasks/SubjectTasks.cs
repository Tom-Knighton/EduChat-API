using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EduChatAPI.Objects;
using MySql.Data.MySqlClient;

namespace EduChatAPI.APITasks
{
    public class SubjectTasks
    {
        string connString = "server=127.0.0.1;port=3306;database=educhat;user=db;password=Tom7494";

        public async Task<List<Subject>> GetAllSubjects(bool excludeAlevels, bool excludeNonEdu)
        {
            using (var conn = new MySqlConnection(connString)) //Using keyword means the variable will be destroyed after scope completed
            {
                await conn.OpenAsync(); // Waits for the connection to open
                List<Subject> subjects = new List<Subject>(); //Initialises a list of subjects
                string sqlString = $"SELECT * FROM subject WHERE IsEnabled=1;"; 
                if (excludeAlevels) sqlString += " AND IsAdvanced = 0";
                if (excludeNonEdu) sqlString += " AND IsEducational = 1"; // ^ Creates the SQL statement
                using (var cmd = new MySqlCommand(sqlString, conn)) //Createsa command with the statement and the connection
                using (var reader = await cmd.ExecuteReaderAsync()) //Creates a reader for the data returned
                    while (await reader.ReadAsync()) //While the reader loops through each row
                    {
                        subjects.Add(await GetSubjectById(Convert.ToInt32(reader["SubjectId"])));
                    }
                return subjects; //return the list
            }
        }

        public async Task<List<Subject>> GetSubscribedSubjects(int Userid)
        {
            using (var conn = new MySqlConnection(connString)) //Creates connection
            {
                await conn.OpenAsync(); //Waits for connection to open
                List<Subject> subjects = new List<Subject>(); //Initialises empty list to return
                string sqlString = $"SELECT * FROM subject_enrollment INNER JOIN subject ON subject_enrollment.SubjectId = subject.SubjectId WHERE subject_enrollment.UserId = {Userid} AND subject_enrollment.IsEnabled = 1;";
                // ^ Selects all rows from subject_enrollment that the user is a part of, and INNER JOINs the actual subject itself for each role
                using (var cmd = new MySqlCommand(sqlString, conn))  //Creates command 
                using (var reader = await cmd.ExecuteReaderAsync()) //Creates reader 
                    while (await reader.ReadAsync()) //While the reader loops through each row
                        subjects.Add(await GetSubjectById(Convert.ToInt32(reader["SubjectId"])));
                return subjects; //returns the subject list
            }
        }

        public async Task<Subject> GetSubjectById(int id)
        {
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                string sqlString = $"SELECT * FROM subject WHERE SubjectId={id};";
                using (var cmd = new MySqlCommand(sqlString, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    if (await reader.ReadAsync())
                    {
                        Subject sub = new Subject
                        {
                            SubjectId = Convert.ToInt32(reader["SubjectId"]),
                            SubjectName = reader["SubjectName"].ToString(),
                            ShortSubjectName = reader["ShortSubjectName"].ToString(),
                            IsEducational = Convert.ToBoolean(reader["IsEducational"]),
                            IsAdvanced = Convert.ToBoolean(reader["IsAdvanced"]),
                            IsEnabled = Convert.ToBoolean(reader["IsEnabled"])
                        };
                        return sub;
                    }
                return null;

            }
        }
    }
}
