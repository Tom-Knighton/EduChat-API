using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace EduChatAPI
{
    public static class Json
    {
        public static T Parse<T>(string json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            T obj = null;

            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };

                obj = JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (Exception ex)
            {
                // TODO: Find a way to log the exception (can shared libraries do that?!)
                obj = null;
            }

            return obj;
        }

        public static string Stringify(object data)
        {
            if (data == null) return null;

            string json = null;

            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };

                json = JsonConvert.SerializeObject(data, settings);
            }
            catch (Exception ex)
            {
                // TODO: Find a way to log the exception (can shared libraries do that?!)
                json = null;
            }

            return json;
        }

        public static string ReaderToJsonString(DbDataReader reader)
        {
            var dataTable = new DataTable();
            dataTable.Load(reader);
            string json = JsonConvert.SerializeObject(dataTable);
            return json;
        }

        
    }

}
