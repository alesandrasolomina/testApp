using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using test;
using System.Xml.Linq;
using System.Reflection;


namespace test
{
    class Program
    {

        static async Task Main(string[] args)
        {
            // configuring db connection 
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();
            string? connectionString = config.GetConnectionString("DefaultConnection");
            var optionsBuilder = new DbContextOptionsBuilder<TestAppContext>();
            var options = optionsBuilder.UseNpgsql(connectionString).Options;

            // getting mode
            string? mode = string.Empty;
            do
            {
                Console.WriteLine(@"введите режим работы( ""импорт"" или ""вывод"")");
                mode = Console.ReadLine().Trim().ToLower();
            }

            while (mode != "импорт" && mode != "вывод");

            // import mode flow
            if (mode == "импорт")
            {
                // getting file type
                string? type = string.Empty;
                do
                {
                    Console.WriteLine(@"введите тип( ""подразделение"" ""сотрудник"" ""должность"")");
                    type = Console.ReadLine().Trim().ToLower();
                }
                while (type != "подразделение" && type != "сотрудник" && type != "должность");

                // getting file name
                string? fileName = string.Empty;
                do
                {
                    Console.WriteLine(@"введите название файла полностью(например, ""departments.tsv"")");
                    fileName = Console.ReadLine().Trim();
                    if (!File.Exists(fileName))
                        Console.WriteLine("Файл не найден");
                }
                while (!File.Exists(fileName));
                

                // initializing import
                if (type == "подразделение")
                {
                    await Methods.ImportingDepartments(options, fileName);

                }

                else if (type == "должность")
                {
                    await Methods.ImportingJobTitles(options, fileName);
                }

                else
                {
                    await Methods.ImportingEmployees(options, fileName);
                }

               Methods.Output(options);
               Console.ReadLine();
            }

            // output mode flow
            else 
            {
                // asking for optional department id parameter

                var depCount = await Methods.GetDepartmentCount(options);
                int depId = -1;
                var r = false;
                do
                {
                    Console.WriteLine(@"введите id подразделения, чтобы посмотреть сотрудников только по подразделению либо ""0""(ноль) для вывода сотрудников по всем подразделениям");
                    var inp = Console.ReadLine();
                    if (Int32.TryParse(inp, out int t) && t>= 0 && t<= depCount)
                    {
                       depId = t;
                       r = true;
                    }
                    
                    else
                    {
                        Console.WriteLine("введите корректный номер подразделения");
                    }
                }
                while (!r);

                if (depId == 0)
                    Methods.Output(options);
                else
                    Methods.OutputById(options, depId);

                Console.ReadLine();
            }

        }

     }
}






