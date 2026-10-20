using Npgsql;
namespace ExpenseTrackerDb;

class Category
{
    public int Id { get; set;  }
    public string Name { get; set; } = "";

}

class Expense
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public string  Category { get; set; } = "";
    public decimal Amount { get; set; }
}

class Program
{
    private static string connString = "";
    static void Main()
    {
        connString = BuildConnectionString();
        bool running = true;

        while (running)
        {
            ShowMenu();
            string choice = Console.ReadLine() ?? "";
            
            try
            {
                switch (choice)
                {
                    case "1":
                        AddExpense();
                        break;
                    case "2":
                        PrintExpenses(GetExpenses());
                        break;
                    case "3":
                        Console.WriteLine($"Total spent: {GetTotal(GetExpenses()):F2}");
                        break;
                    case "4":
                        ShowCategoryTotals();
                        break;
                    case "5":
                        DeleteExpense();
                        break;
                    case "6":
                        running = false;
                        Console.WriteLine("Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please enter 1-6.");
                        break;
                }
            }
            catch (PostgresException ex)
            {
                Console.WriteLine($"Database says {ex.MessageText} ");
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Could not reach the database: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    static string BuildConnectionString()
    {
        string? password = Environment.GetEnvironmentVariable("EXPENSE_DB_PASSWORD");
        if (string.IsNullOrEmpty(password))
        {
            Console.Write("Database password for user postgres: ");
            password = Console.ReadLine() ?? "";
        }
        return $"Host=localhost;Port=5432;Database=expense_tracker;Username=postgres;Password={password}";
    }

    static void ShowMenu()
    {
        Console.WriteLine("=== Expense Tracker (PostgreSQL) ===");
        Console.WriteLine("1. Add expense");
        Console.WriteLine("2. View all expenses");
        Console.WriteLine("3. View total");
        Console.WriteLine("4. Totals by category");
        Console.WriteLine("5. Delete expense");
        Console.WriteLine("6. Exit");
        Console.Write("Choose an option: ");
    }

    static List<Category> GetCategories()
    {
        List<Category> categories = new List<Category>();

        using NpgsqlConnection conn = new NpgsqlConnection(connString);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT id, name FROM categories ORDER BY id", conn);
        using NpgsqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            categories.Add(new Category { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        }
        return categories;
    }

    static List<Expense> GetExpenses()
    {
        List<Expense> expenses = new List<Expense>();

        using NpgsqlConnection conn = new NpgsqlConnection(connString);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand(
            "SELECT id, description, amount, category FROM expense_details ORDER BY id", conn);
        using NpgsqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            expenses.Add(new Expense
            {
                Id = reader.GetInt32(0),
                Description = reader.GetString(1),
                Amount = reader.GetDecimal(2),
                Category = reader.GetString(3)
            });
        }
        return expenses;
    }
    static void AddExpense()
    {
        Console.Write("Description: ");
        string description = Console.ReadLine() ?? "";

        Console.Write("Amount: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
        {
            Console.WriteLine("Invalid amount. Expense not added.");
            return;
        }

        int categoryId = ChooseCategory();

        using NpgsqlConnection conn = new NpgsqlConnection(connString);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("CALL add_expense(@description, @amount, @categoryId)", conn);
        cmd.Parameters.AddWithValue("description", description);
        cmd.Parameters.AddWithValue("amount", amount);
        cmd.Parameters.AddWithValue("categoryId", categoryId);
        cmd.ExecuteNonQuery();

        Console.WriteLine("Expense added.");
    }

    static void DeleteExpense()
    {
        List<Expense> expenses = GetExpenses();
        if (expenses.Count == 0)
        {
            Console.WriteLine("No expenses to delete.");
            return;
        }

        PrintExpenses(expenses);
        Console.Write("Enter the ID of the expense to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID. Nothing deleted.");
            return;
        }

        using NpgsqlConnection conn = new NpgsqlConnection(connString);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("CALL delete_expense(@id)", conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();

        Console.WriteLine("Expense deleted.");
    }
    static int ChooseCategory()
    {
        List<Category> categories = GetCategories();

        Console.WriteLine("Choose a category:");
        for (int i = 0; i < categories.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {categories[i].Name}");
        }

        Console.Write("Category number: ");
        if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= categories.Count)
        {
            return categories[choice - 1].Id;
        }

        Console.WriteLine("Invalid choice. Using the last category ('Other').");
        return categories[categories.Count - 1].Id;
    }

    static void PrintExpenses(List<Expense> expenses)
    {
        if (expenses.Count == 0)
        {
            Console.WriteLine("No expenses yet.");
            return;
        }

        foreach (Expense e in expenses)
        {
            Console.WriteLine($"{e.Id}. {e.Description} - {e.Amount:F2} ({e.Category})");
        }
    }

    static decimal GetTotal(List<Expense> expenses)
    {
        decimal total = 0;
        foreach (Expense e in expenses)
        {
            total += e.Amount;
        }
        return total;
    }
    static void ShowCategoryTotals()
    {
        using NpgsqlConnection conn = new NpgsqlConnection(connString);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT category, total FROM category_totals()", conn);
        using NpgsqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Console.WriteLine($"{reader.GetString(0)}: {reader.GetDecimal(1):F2}");
        }
    }
}



