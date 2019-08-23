using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EduChatAPI.Objects;
using MySql.Data.MySqlClient;

namespace EduChatAPI.APITasks
{
    public class SubjectTasks
    {
        MySqlConnection conn = new MySqlConnection("server=127.0.0.1;port=3306;database=educhat;user=db;password=Tom7494");

        public async Task<List<Subject>> GetAllSubjects(bool excludeAlevels, bool excludeNonEdu)
        {
            List<Subject> subjects = new List<Subject>();
            await conn.OpenAsync();
            string sqlString = $"SELECT * FROM subject WHERE IsEnabled=1";
            if (excludeAlevels) sqlString += " AND IsAdvanced = 0";
            if (excludeNonEdu) sqlString += " AND IsEducational = 1";

            MySqlDataReader reader = new MySqlCommand($"{sqlString};", conn).ExecuteReader(); 
            while (reader.Read())
            {
                subjects.Add(new Subject
                {
                    SubjectId = Convert.ToInt32(reader["SubjectId"]),
                    SubjectName = reader["SubjectName"].ToString(),
                    IsEducational = Convert.ToBoolean(reader["IsEducational"]),
                    IsAdvanced = Convert.ToBoolean(reader["IsAdvanced"]),
                    IsEnabled = Convert.ToBoolean(reader["IsEnabled"])
                });
            }
            conn.Close();
            return subjects;
        }

        public async Task<List<Subject>> GetSubscribedSubjects(int Userid)
        {
            List<Subject> subjects = new List<Subject>();
            await conn.OpenAsync();
            string sqlString = $"SELECT * FROM subject_enrollment INNER JOIN subject ON subject_enrollment.SubjectId = subject.SubjectId WHERE subject_enrollment.UserId = {Userid} AND subject_enrollment.IsEnabled = 1;";
            MySqlDataReader reader = new MySqlCommand($"{sqlString}", conn).ExecuteReader();
            while (reader.Read())
            {
                subjects.Add(new Subject
                {
                    SubjectId = Convert.ToInt32(reader["SubjectId"]),
                    SubjectName = reader["SubjectName"].ToString(),
                    IsEducational = Convert.ToBoolean(reader["IsEducational"]),
                    IsAdvanced = Convert.ToBoolean(reader["IsAdvanced"]),
                    IsEnabled = Convert.ToBoolean(reader["IsEnabled"])
                });
            }
            conn.Close();
            return subjects;
        }

        public async Task<Subject> GetSubjectById(int id)
        {
            await conn.OpenAsync();
            MySqlDataReader reader = new MySqlCommand($"SELECT * FROM subject WHERE SubjectId={id};", conn).ExecuteReader();
            if (reader.Read())
            {
                Subject sub = new Subject
                {
                    SubjectId = Convert.ToInt32(reader["SubjectId"]),
                    SubjectName = reader["SubjectName"].ToString(),
                    IsEducational = Convert.ToBoolean(reader["IsEducational"]),
                    IsAdvanced = Convert.ToBoolean(reader["IsAdvanced"]),
                    IsEnabled = Convert.ToBoolean(reader["IsEnabled"])
                };
                conn.Close();
                return sub;
            }
            else { conn.Close();  return null; }
        }

    }
}
