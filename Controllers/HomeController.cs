using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utils.AIUA01.Data;
using Utils.AIUA01.Models;


namespace Utils.AIUA01.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _configuration=configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Action method to generate models
    [HttpPost]
    public IActionResult GenerateModels()
    {
        try
        {
            // Get all tables from the database
            var tables = _context.Model.GetEntityTypes().ToList();
            
            //tables.FirstOrDefault().GetTableName();
            
            // Path to the Models folder
            string modelsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Models");

            // Ensure Models folder exists, create if not
            if (!Directory.Exists(modelsFolderPath))
            {
                Directory.CreateDirectory(modelsFolderPath);
            }

            // Log file path
            string logPath = Path.Combine(Directory.GetCurrentDirectory(), "ModelLog.txt");
            StringBuilder logContent = new StringBuilder();

            foreach (var table in tables)
            {
                var className = table.ClrType.Name;  // Table name becomes class name
                var columns = table.GetProperties();  // Get columns for the table

                // Begin class generation
                StringBuilder classContent = new StringBuilder();
                classContent.AppendLine("using System;");
                classContent.AppendLine("using System.ComponentModel.DataAnnotations;");
                classContent.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
                classContent.AppendLine();
                classContent.AppendLine($"namespace YourNamespace.Models"); // You can change namespace
                classContent.AppendLine("{");
                classContent.AppendLine($"    [Table(\"{table.GetTableName()}\")]");
                classContent.AppendLine($"    public class {className}");
                classContent.AppendLine("    {");

                // Iterate through columns and generate properties
                foreach (var column in columns)
                {
                    var columnName = column.Name;
                    var columnType = GetClrType(column.ClrType); // Map DB types to C# types

                    classContent.AppendLine($"        [Column(\"{columnName}\")]");
                    classContent.AppendLine($"        public {columnType} {columnName} {{ get; set; }}");
                    classContent.AppendLine();
                }

                classContent.AppendLine("    }");
                classContent.AppendLine("}");

                // Write the class file to the Models folder
                string classFilePath = Path.Combine(modelsFolderPath, $"{className}.cs");
                System.IO.File.WriteAllText(classFilePath, classContent.ToString());

                // Log the model creation
                logContent.AppendLine($"Model created for table: {className}");
            }

            // Save the log file
            System.IO.File.WriteAllText(logPath, logContent.ToString());

            // Optionally return the log file as a download response
            return File(System.IO.File.ReadAllBytes(logPath), "text/plain", "ModelLog.txt");
        }
        catch (Exception ex)
        {
            return Content($"Error: {ex.Message}");
        }
    }

    // Helper method to map MySQL types to C# types
    private string GetClrType(Type type)
    {
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(DateTime)) return "DateTime";
        if (type == typeof(string)) return "string";
        // Add more mappings as needed

        return "object"; // Default for unsupported types
    }

    // Function to get the list of tables in the current database and generate models
    public void GenerateModelsFromTables()
    {
        // Get the list of tables in the current database using FromSqlRaw
        var tables = _context.TableInfos
            .FromSqlRaw("SELECT table_name AS TableName FROM information_schema.tables WHERE table_schema = DATABASE() AND table_type = 'BASE TABLE'")
            .ToList();

        // Ensure the Models folder exists
        string modelsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Models\\GenDBModels");
        if (!Directory.Exists(modelsFolderPath))
        {
            Directory.CreateDirectory(modelsFolderPath);
        }

        foreach (var table in tables)
        {
            string tableName = table.TableName;
            if(!tableName.StartsWith("AspNet"))
            GenerateModelForTable(tableName, modelsFolderPath);
        }
    }

    // Function to generate a model for a specific table
    private void GenerateModelForTable(string tableName, string modelsFolderPath)
    {
        // Get the columns and their types for the specified table using FromSqlRaw
        var columns = _context.ColumnInfos
            .FromSqlRaw(@$"SELECT column_name AS ColumnName, data_type AS DataType, is_nullable AS IsNullable 
                           FROM information_schema.columns 
                           WHERE table_schema = DATABASE() 
                           AND table_name = '{tableName}'")
            .ToList();

        // Create class file content
        StringBuilder classContent = new StringBuilder();
        classContent.AppendLine("using System;");
        classContent.AppendLine("using System.ComponentModel.DataAnnotations;");
        classContent.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
        classContent.AppendLine();
        classContent.AppendLine($"namespace AIModelGen.Models");  // Use your desired namespace
        classContent.AppendLine("{");
        //classContent.AppendLine($"    [Table(\"{tableName}\")]");
        classContent.AppendLine($"    public class {tableName}");
        classContent.AppendLine("    {");

        // Add properties for each column
        foreach (var column in columns)
        {
            string columnName = column.ColumnName;
            string columnType = column.DataType;
            bool isNullable = column.IsNullable == "YES";

            string clrType = GetClrType(columnType, isNullable);
            if(columnName.Equals("Id") ) classContent.AppendLine($"        [Key]");
            //classContent.AppendLine($"        [Column(\"{columnName}\")]");
            classContent.AppendLine($"        public {clrType} {columnName} {{ get; set; }}");
            classContent.AppendLine();
        }

        classContent.AppendLine("    }");
        classContent.AppendLine("}");

        // Write the class to the Models folder
        string classFilePath = Path.Combine(modelsFolderPath, $"{tableName}.cs");
        System.IO.File.WriteAllText(classFilePath, classContent.ToString());
    }

    // Helper method to map MySQL data types to C# types
    private string GetClrType(string sqlType, bool isNullable)
    {
        string clrType = sqlType.ToLower() switch
        {
            "int" => "int",
            "bigint" => "long",
            "decimal" => "decimal",
            "double" => "double",
            "float" => "float",
            "varchar" => "string",
            "char" => "string",
            "text" => "string",
            "datetime" => "DateTime",
            "date" => "DateTime",
            "bit" => "bool",
            _ => "object" // Default for unsupported types
        };

        if (isNullable && clrType != "string") // Nullable types for value types
        {
            clrType += "?";
        }

        return clrType;
    }

}
