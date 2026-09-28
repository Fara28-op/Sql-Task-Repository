using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace SqlTaskRepository
{
    // Модель завдання (ООП структура)
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
    }

    class Program
    {
        // Шлях до файлу локальної бази даних SQL
        private static readonly string connectionString = "Data Source=tasks.db";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            InitializeDatabase();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== SQL Connected Task Manager (CRUD) ===");
                Console.WriteLine("1. View All Tasks (SQL SELECT)");
                Console.WriteLine("2. Create New Task (SQL INSERT)");
                Console.WriteLine("3. Delete Task by ID (SQL DELETE)");
                Console.WriteLine("4. Exit");
                Console.Write("\nChoose an SQL operation: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        ReadTasks();
                        break;
                    case "2":
                        CreateTask();
                        break;
                    case "3":
                        DeleteTask();
                        break;
                    case "4":
                        Console.WriteLine("\n[System] Disconnected from SQL Database. Goodbye!");
                        return;
                    default:
                        Console.WriteLine("\n[Error] Invalid choice. Press Enter.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        // Автоматичне проектування таблиці в базі даних при старті
        private static void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT EXISTS,
                        Status TEXT NOT EXISTS
                    );";

                using (var command = new SqliteCommand(createTableSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // Операція READ: Вибірка даних (SQL SELECT)
        private static void ReadTasks()
        {
            Console.Clear();
            Console.WriteLine("--- Executing: SELECT * FROM Tasks --- \n");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string selectSql = "SELECT Id, Title, Status FROM Tasks";

                using (var command = new SqliteCommand(selectSql, connection))
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("SQL Result: 0 rows returned. Database is empty.");
                    }
                    else
                    {
                        Console.WriteLine("ID  | Status     | Task Title");
                        Console.WriteLine("-----------------------------------");
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string title = reader.GetString(1);
                            string status = reader.GetString(2);
                            Console.WriteLine($"{id,-3} | {status,-10} | {title}");
                        }
                    }
                }
            }
            Console.WriteLine("\nPress Enter to return.");
            Console.ReadLine();
        }

        // Операція CREATE: Запис у базу (SQL INSERT)
        private static void CreateTask()
        {
            Console.Clear();
            Console.WriteLine("--- Executing: INSERT INTO Tasks --- \n");
            Console.Write("Enter task title: ");
            string title = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("[Error] Title cannot be empty.");
                Console.ReadLine();
                return;
            }

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string insertSql = "INSERT INTO Tasks (Title, Status) VALUES (@title, @status)";

                using (var command = new SqliteCommand(insertSql, connection))
                {
                    // Безпечна параметризація запитів для захисту від SQL-ін'єкцій
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@status", "Pending");
                    command.ExecuteNonQuery();
                }
            }

            Console.WriteLine("\n[Success] Row inserted into SQL database! Press Enter.");
            Console.ReadLine();
        }

        // Операція DELETE: Видалення з бази за ID (SQL DELETE)
        private static void DeleteTask()
        {
            Console.Clear();
            Console.WriteLine("--- Executing: DELETE FROM Tasks WHERE Id = @id --- \n");
            Console.Write("Enter Task ID to delete: ");

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("[Error] Invalid numeric ID.");
                Console.ReadLine();
                return;
            }

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string deleteSql = "DELETE FROM Tasks WHERE Id = @id";

                using (var command = new SqliteCommand(deleteSql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        Console.WriteLine($"\n[Success] Row with ID {id} deleted successfully!");
                    else
                        Console.WriteLine($"\n[Warning] No row found with ID {id}.");
                }
            }
            Console.WriteLine("\nPress Enter to return.");
            Console.ReadLine();
        }
    }
}
