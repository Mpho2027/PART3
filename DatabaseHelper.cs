using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace taskChatt
{
    // ---------------------------------------------------------------
    // Model class for a Task
    // ---------------------------------------------------------------
    public class CyberTask
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsCompleted { get; set; }
        public DateTime? ReminderDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public override string ToString()
        {
            string status = IsCompleted ? "✅" : "⏳";
            string reminder = ReminderDate.HasValue
                ? $" | Reminder: {ReminderDate.Value:dd MMM yyyy}"
                : "";
            return $"{status} [{TaskId}] {Title}{reminder}";
        }
    }

    // ---------------------------------------------------------------
    // Database helper — change the Database= value if your DB has a
    // different name.  Server=(localdb)\\MSSQLLocalDB covers VS default.
    // ---------------------------------------------------------------
    public static class DatabaseHelper
    {
        // *** Update Database= to match the name you created in SSMS ***
        private const string ConnectionString =
            @"Server=(localdb)\MSSQLLocalDB;Database=TaskChat;Trusted_Connection=True;TrustServerCertificate=True;";

        // ── CREATE ──────────────────────────────────────────────────
        public static bool AddTask(string title, string description, DateTime? reminderDate)
        {
            try
            {
                using SqlConnection conn = new(ConnectionString);
                conn.Open();
                string sql = @"INSERT INTO Tasks (Title, Description, ReminderDate)
                               VALUES (@title, @desc, @reminder)";
                using SqlCommand cmd = new(sql, conn);
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@desc", description);
                cmd.Parameters.AddWithValue("@reminder",
                    (object?)reminderDate ?? DBNull.Value);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("DB error (AddTask): " + ex.Message);
                return false;
            }
        }

        // ── READ ─────────────────────────────────────────────────────
        public static List<CyberTask> GetAllTasks()
        {
            var list = new List<CyberTask>();
            try
            {
                using SqlConnection conn = new(ConnectionString);
                conn.Open();
                string sql = "SELECT * FROM Tasks ORDER BY CreatedAt DESC";
                using SqlCommand cmd = new(sql, conn);
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new CyberTask
                    {
                        TaskId      = reader.GetInt32("TaskId"),
                        Title       = reader.GetString("Title"),
                        Description = reader.GetString("Description"),
                        IsCompleted = reader.GetBoolean("IsCompleted"),
                        ReminderDate = reader.IsDBNull("ReminderDate")
                                        ? null
                                        : reader.GetDateTime("ReminderDate"),
                        CreatedAt   = reader.GetDateTime("CreatedAt")
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("DB error (GetAllTasks): " + ex.Message);
            }
            return list;
        }

        // ── COMPLETE ─────────────────────────────────────────────────
        public static bool MarkCompleted(int taskId)
        {
            try
            {
                using SqlConnection conn = new(ConnectionString);
                conn.Open();
                string sql = "UPDATE Tasks SET IsCompleted = 1 WHERE TaskId = @id";
                using SqlCommand cmd = new(sql, conn);
                cmd.Parameters.AddWithValue("@id", taskId);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("DB error (MarkCompleted): " + ex.Message);
                return false;
            }
        }

        // ── DELETE ───────────────────────────────────────────────────
        public static bool DeleteTask(int taskId)
        {
            try
            {
                using SqlConnection conn = new(ConnectionString);
                conn.Open();
                string sql = "DELETE FROM Tasks WHERE TaskId = @id";
                using SqlCommand cmd = new(sql, conn);
                cmd.Parameters.AddWithValue("@id", taskId);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("DB error (DeleteTask): " + ex.Message);
                return false;
            }
        }

        // ── TEST CONNECTION ──────────────────────────────────────────
        public static bool TestConnection()
        {
            try
            {
                using SqlConnection conn = new(ConnectionString);
                conn.Open();
                return true;
            }
            catch { return false; }
        }
    }
}
