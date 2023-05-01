using System;
using System.Runtime.Intrinsics.Arm;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace test 
{

    public class Methods
    {
        public static async void OutputById(DbContextOptions<TestAppContext> options, int depId)
        {
            var opt = options;
            List<string> ancestors = new List<string>();
            var dep = await GetDep(depId, opt);
            ancestors = await GetAncestors(opt, dep, ancestors);
           
            ancestors.Reverse();
            foreach (var an in ancestors)
            {
               Console.WriteLine(an);
            }
            Consoling(opt, dep);
        }
        static async Task<List<string>> GetAncestors(DbContextOptions<TestAppContext> opt, Department dep, List<string> ancestors)
        {
            var res = await GetParent(opt, dep);
            if (res != null) 
            {
               ancestors.Add(res.Name);
               await GetAncestors(opt, res, ancestors);
            }
            return ancestors;
        }

        static async Task<Department> GetParent(DbContextOptions<TestAppContext> opt, Department dept)
        {
            using (var db = new TestAppContext(opt))
            {
                return await db.Departments.FirstOrDefaultAsync(x => x.Id == dept.Parentid);
            }
        }

        public static async void Output(DbContextOptions<TestAppContext> options)
        {
            var opt = options;

            static async Task<IEnumerable<Department>> GetMainDepartments(DbContextOptions<TestAppContext> opt)
            {
                using (var db = new TestAppContext(opt))
                {
                    return await db.Departments.Where(x => x.Parentid == null).OrderBy(x => x.Name).ToListAsync();
                }
            }

            var mainDep = await GetMainDepartments(opt);

            foreach (var department in mainDep)
            {
                await DepartmentOutput(opt, department);
            }
        }

        public static async Task DepartmentOutput(DbContextOptions<TestAppContext> options, Department dep, int nest = 0)
        {
            var opt = options;
            var children = await GetChildren(opt, dep);
            Consoling(opt, dep, nest);
            foreach (var child in children)
            {
                await NestCheckin(opt, child, nest);
            }
        }


        static async Task NestCheckin(DbContextOptions<TestAppContext> opt, Department dep, int nest = 0)
        {
            var children = await GetChildren(opt, dep);
            if (children.Count == 0)
                {
                    Consoling(opt, dep, nest);
                }
                else
                {
                    Consoling(opt, dep, nest);
                    await NestCheckin(opt, children[0], nest + 1);
                }
        }


        static async void Consoling(DbContextOptions<TestAppContext> opt, Department dep, int nest = 0)
        {
            Console.WriteLine(new string('=', nest) + " " + dep.Name);
            var employees = await GetEmployees(opt, dep);

            if (dep.Managerid != null)
                Console.WriteLine(new string(' ', nest) + "* " + dep.ManagerName);
 
            foreach (var employee in employees)
            {
                if (employee.Fullname == dep.ManagerName)
                    continue;
                Console.WriteLine(new string(' ', nest) + "- " + employee.Fullname);
            }
        }

        static async Task<IEnumerable<Employee>> GetEmployees(DbContextOptions<TestAppContext> opt, Department dept)
        {
            using (var db = new TestAppContext(opt))
            {
                return await db.Employees.Where(x => x.Department == dept.Id).OrderBy(x => x.Fullname).ToListAsync();
            }
        }


        static async Task<List<Department>> GetChildren(DbContextOptions<TestAppContext> opt, Department dept)
        {
            using (var db = new TestAppContext(opt))
            {
                return await db.Departments.Where(x => x.Parentid == dept.Id).OrderBy(x => x.Name).ToListAsync();
            }
        }


        public static async Task ImportingEmployees(DbContextOptions<TestAppContext> options, string fileName)
        {
            var opt = options;

            IEnumerable<string> lines = File.ReadLines(fileName);
            int i = 0;
            foreach (var line in lines)
            {
                //counter to eliminate the headers
                if (i == 0)
                {
                    i++;
                    continue;
                }

                var fields = line.Split("\t");


                var employee = new Employee();
                try
                {
                    employee.Id = await GetEmployeeCount(options) + 1;
                    employee.Fullname = fields[1].Trim();
                    employee.Login = fields[2];
                    employee.Password = fields[3];

                    var department = await GetIdByName(fields[0].Trim(), opt);
                    if (department != null)
                        employee.Department = department.DepartmentId;

                    var jobtitle = await GetJobtitleByName(fields[4].Trim(), opt);
                    if (jobtitle != null)
                        employee.Jobtitle = jobtitle.Id;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                await AddEmployee(employee, opt);

                var depList = await GetDepartmentsByManagerName(employee.Fullname, opt);
                foreach (var dep in depList)
                {
                    await UpdateDepartmentValues(employee.Id, dep, opt);
                }
                i++;
            }

            Console.WriteLine("сотрудники импортированы успешно");

        }


        static async Task UpdateDepartmentValues(int managerId, Department dep, DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
                var d = await db.Departments.FindAsync(dep.Id);
                d.Managerid = managerId;

                await db.SaveChangesAsync();
            }
        }


        public static async Task ImportingDepartments(DbContextOptions<TestAppContext> options, string fileName)
        {
            var opt = options;

            IEnumerable<string> lines = File.ReadLines(fileName);
            int i = 0;
            foreach (var line in lines)
            {
                //counter to eliminate the headers
                if (i == 0)
                {
                    i++;
                    continue;
                }

                var fields = line.Split("\t");

                var depName = new DepartmentsName();
                try
                {
                    depName.Name = fields[0].Trim();
                    depName.DepartmentId = await GetDepartmentCount(options) + 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                await AddDepName(depName, opt);

                var dep = new Department();
                try
                {
                    dep.Id = depName.DepartmentId;
                    dep.Name = depName.Name;

                    var parentId = await GetIdByName(fields[1].Trim(), opt);
                    if (parentId != null)
                        dep.Parentid = parentId.DepartmentId;


                    dep.Phone = fields[3];
                    dep.ManagerName = fields[2].Trim();
                    var managerid = await GetManagerIdByName(fields[2].Trim(), opt);
                    if (managerid != null)
                        dep.Managerid = managerid.Id;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                await AddDep(dep, opt);

                i++;
            }
                        
            Console.WriteLine("подразделения импортированы успешно");
        }

        public static async Task ImportingJobTitles(DbContextOptions<TestAppContext> options, string fileName)
        {
            var opt = options;

            IEnumerable<string> lines = File.ReadLines(fileName);
            int i = 0;
            foreach (var line in lines)
            {
                //counter to eliminate the headers
                if (i == 0)
                {
                    i++;
                    continue;
                }

                var fields = line.Split("\t");
                foreach (var f in fields)
                    f.Trim();
                var jobTitle = new Jobtitle();
                try
                {
                    jobTitle.Name = fields[0].Trim();
                    jobTitle.Id = await GetJobtitlesCount(options) + 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                await AddJobTitle(jobTitle, opt);

                i++;

            }

            Console.WriteLine("должности импортированы успешно");

        }


        static async Task AddDep(Department dep, DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
                var res = db.Departments.Any(t => t.Name == dep.Name);
                if (res == false)
                {
                    await db.Departments.AddAsync(dep);
                    await db.SaveChangesAsync();
                }
            }
        }

        static async Task<List<Department>> GetDepartmentsByManagerName(string managerName, DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
                return await db.Departments.Where(t => t.ManagerName == managerName && t.Managerid == null).ToListAsync();
            }
        }

        static async Task AddJobTitle(Jobtitle jobTitle, DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
                var res = db.Jobtitles.Any(t => t.Name == jobTitle.Name);
                if (res == false)
                {
                    await db.Jobtitles.AddAsync(jobTitle);
                    await db.SaveChangesAsync();
                }
            }
        }

        static async Task AddEmployee(Employee employee, DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
               var res = db.Employees.Any(t => t.Department == employee.Department && t.Fullname == employee.Fullname && t.Jobtitle == employee.Jobtitle);
                if (res == false)
                {
                    await db.Employees.AddAsync(employee);
                    await db.SaveChangesAsync();
                }
            }
        }

        static async Task AddDepName(DepartmentsName depName, DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
                var res = db.DepartmentsNames.Any(t => t.Name == depName.Name);
                if (res == false)
                {
                    await db.DepartmentsNames.AddAsync(depName);
                    await db.SaveChangesAsync();
                }
            }
        }

        public static async Task<int> GetDepartmentCount(DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
                return await db.Departments.CountAsync();
            }
        }

        static async Task<int> GetEmployeeCount(DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
                return await db.Employees.CountAsync();
            }
        }

        static async Task<int> GetJobtitlesCount(DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
                return await db.Jobtitles.CountAsync();
            }
        }

        static async Task<Department> GetDep(int id, DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
                return await db.Departments.FindAsync(id);
            }
        }


        static async Task<DepartmentsName> GetIdByName(string name, DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
                var list = db.DepartmentsNames.FirstOrDefaultAsync(x => x.Name == name);
                return await list;
            }
        }

        static async Task<Jobtitle> GetJobtitleByName(string name, DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
                return await db.Jobtitles.FirstOrDefaultAsync(x => x.Name == name);
            }
        }
        static async Task<Employee> GetManagerIdByName(string name, DbContextOptions<TestAppContext> opt)
        {
            using (var db = new TestAppContext(opt))
            {
                return await db.Employees.FirstOrDefaultAsync(x => x.Fullname == name);
            }
        }
    }

}
