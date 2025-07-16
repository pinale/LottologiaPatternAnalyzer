using LottologiaPatternAnalyzer.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LottologiaPatternAnalyzer.Services
{
    public class SqliteService
    {
        string _dbPath;
        public SqliteService(string dbPath)
        {
            _dbPath = dbPath;   
        }

        public bool InsertData(List<TableEntry> table, bool overWrite = false)
        {
            using (SqliteConnection _conn = new SqliteConnection(_dbPath))
            {
                _conn.Open();
                using (SqliteTransaction trans = _conn.BeginTransaction())
                using (SqliteCommand command = _conn.CreateCommand())
                {
                    foreach (TableEntry entry in table)
                    {
                        if (overWrite == true)
                        {
                            //delete
                            string checkSql = $"DELETE FROM StoricoPattern WHERE DrawDate='{entry.DrawDate}' AND DrawNumber={entry.DrawNumber} AND Code='{entry.Code}' AND Objective={entry.Objective}";
                            command.CommandText = checkSql;
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            string checkSql = $"SELECT EXISTS (SELECT 1 FROM StoricoPattern WHERE DrawDate='{entry.DrawDate}' AND DrawNumber={entry.DrawNumber} AND Code='{entry.Code}' AND Objective={entry.Objective})";
                            command.CommandText = checkSql;
                            if (Convert.ToBoolean(command.ExecuteScalar()))
                                break;
                        }

                        command.CommandText = GetInsertCommand();
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@DrawDate", entry.DrawDate);
                        command.Parameters.AddWithValue("@DrawTime", entry.DrawTime);
                        command.Parameters.AddWithValue("@DrawNumber", entry.DrawNumber);
                        command.Parameters.AddWithValue("@DrawFullTimeStamp", entry.DrawFullTimeStamp);
                        command.Parameters.AddWithValue("@Code", entry.Code);
                        command.Parameters.AddWithValue("@Objective", entry.Objective);
                        command.Parameters.AddWithValue("@RrStandard", entry.RrStandard);
                        command.Parameters.AddWithValue("@RrExtra", entry.RrExtra);
                        command.Parameters.AddWithValue("@RrCombined", entry.RrCombined);

                        var result = command.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
            }

            return true;
        }
        private string GetInsertCommand()
        {
            string sFields = "DrawDate, DrawTime, DrawNumber, DrawFullTimeStamp, Code, Objective, RrStandard, RrExtra, RrCombined";
            string sParameters = "@DrawDate, @DrawTime, @DrawNumber, @DrawFullTimeStamp, @Code, @Objective, @RrStandard, @RrExtra, @RrCombined";
            string sql = $"INSERT INTO StoricoPattern ({sFields}) VALUES ({sParameters})";
            return sql;
        }

        public DataTable GetDrawDay(DateTime date) { 
            string sDate = date.ToString("yyyy-MM-dd");
            DataTable _storico = new DataTable();
            string sql = $"SELECT * FROM V_StoricoEstrazioni where Data='{sDate}'";

            using (SQLiteConnection _conn = new SQLiteConnection(_dbPath))
            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, _conn))
            {
                adapter.Fill(_storico);
            }
            return _storico;
        }

        public Dictionary<string, string> GetSerieByCode(string code)
        {
            Dictionary<string, string> series = new Dictionary<string, string>();
            string sql = $"SELECT Codice, Serie FROM CatalogoPattern WHERE Codice LIKE'{code.Replace("_",@"\_")}' ESCAPE '\\'";

            using (SqliteConnection _conn = new SqliteConnection(_dbPath))
            {
                _conn.Open();
                using (SqliteCommand command = new SqliteCommand(sql, _conn))
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        series.Add(reader["Codice"].ToString(),reader["Serie"].ToString());
                    }
                }
            }

            return series;
        }
    }
}
