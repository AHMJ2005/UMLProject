using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UMLCODEC_
{
    // DB Records
    public class Card
    {
        public string CardNumber { get; set; } = "";
        public string CardType { get; set; } = "";   // "student" or "faculty member"
        public double Balance { get; set; }
        public string Status { get; set; } = "";     // "unblocked" or "blocked"
        public string UserId { get; set; } = "";

        public string GetDetails() =>
            $"Card #{CardNumber} | Type: {CardType} | Balance: {Balance:F2} JD | Status: {Status} | User: {UserId}";
    }

    public class Transaction
    {
        public string TransactionId { get; set; } = "";
        public string CardNumber { get; set; } = ""; // REQUIRED by spec
        public string TransactionType { get; set; } = ""; // "recharge" / "attendance" / "payment"
        public double? Amount { get; set; } // N/A for attendance
        public string Date { get; set; } = ""; // yyyy-MM-dd
        public string UserId { get; set; } = "";

        public string GetDetails()
        {
            string amountStr = Amount.HasValue ? Amount.Value.ToString("0.##") : "N/A";
            return $"ID: {TransactionId} | Card: {CardNumber} | Type: {TransactionType} | Amount: {amountStr} | Date: {Date} | User: {UserId}";
        }
    }

    public class AttendanceRecord
    {
        public string CourseId { get; set; } = "";
        public string Date { get; set; } = "";
        public List<string> AttendeeIds { get; set; } = new List<string>();
    }

    public class StudentData
    {
        public string UserId { get; set; } = "";
        public string Name { get; set; } = "";
        public List<string> RegisteredCourses { get; set; } = new List<string>();
    }

    public class FacultyData
    {
        public string UserId { get; set; } = "";
        public string Name { get; set; } = "";
        public List<string> TaughtCourses { get; set; } = new List<string>();
    }

    // File-based "Database"
    public class SystemManager
    {
        private const string CardsFile = "cards.json";
        private const string TransactionsFile = "transactions.json";
        private const string AttendanceFile = "attendance.json";
        private const string StudentsFile = "students.json";
        private const string FacultyFile = "faculty.json";

        public SystemManager()
        {
            InitializeFile(CardsFile);
            InitializeFile(TransactionsFile);
            InitializeFile(AttendanceFile);
            InitializeFile(StudentsFile);
            InitializeFile(FacultyFile);

            SeedIfFirstRun();
        }

        private static void InitializeFile(string file)
        {
            if (!File.Exists(file))
                File.WriteAllText(file, "[]");
        }

        private static List<T> LoadData<T>(string file)
        {
            string json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }

        private static void SaveData<T>(string file, List<T> data)
        {
            var opt = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(file, JsonSerializer.Serialize(data, opt));
        }

        // ---- Cards ----
        public List<Card> GetCards() => LoadData<Card>(CardsFile);

        public void SaveCard(Card card)
        {
            var cards = GetCards();
            int idx = cards.FindIndex(c => c.CardNumber == card.CardNumber);
            if (idx != -1) cards[idx] = card;
            else cards.Add(card);
            SaveData(CardsFile, cards);
        }

        // ---- Transactions ----
        public List<Transaction> GetTransactions() => LoadData<Transaction>(TransactionsFile);

        public void AddTransaction(Transaction tr)
        {
            var trs = GetTransactions();
            trs.Add(tr);
            SaveData(TransactionsFile, trs);
        }

        // ---- Attendance ----
        public List<AttendanceRecord> GetAttendance() => LoadData<AttendanceRecord>(AttendanceFile);

        public void SaveAttendance(AttendanceRecord record)
        {
            var records = GetAttendance();
            int idx = records.FindIndex(r => r.CourseId == record.CourseId && r.Date == record.Date);

            if (idx != -1) records[idx] = record;
            else records.Add(record);

            SaveData(AttendanceFile, records);
        }

        // ---- Students & Faculty ----
        public List<StudentData> GetStudents() => LoadData<StudentData>(StudentsFile);
        public List<FacultyData> GetFaculty() => LoadData<FacultyData>(FacultyFile);

        public StudentData? FindStudent(string userId) => GetStudents().FirstOrDefault(s => s.UserId == userId);
        public FacultyData? FindFaculty(string userId) => GetFaculty().FirstOrDefault(f => f.UserId == userId);

        public void SaveStudent(StudentData s)
        {
            var students = GetStudents();
            int idx = students.FindIndex(x => x.UserId == s.UserId);
            if (idx != -1) students[idx] = s;
            else students.Add(s);
            SaveData(StudentsFile, students);
        }

        public void SaveFaculty(FacultyData f)
        {
            var faculty = GetFaculty();
            int idx = faculty.FindIndex(x => x.UserId == f.UserId);
            if (idx != -1) faculty[idx] = f;
            else faculty.Add(f);
            SaveData(FacultyFile, faculty);
        }

        private void SeedIfFirstRun()
        {
            // If students file is empty => first run => load everything as required
            if (GetStudents().Count > 0 || GetFaculty().Count > 0 || GetCards().Count > 0)
                return;

            // data loaded students
            var students = new List<StudentData>
            {
                new StudentData { UserId="S01", Name="Ali",   RegisteredCourses=new List<string>{"CPE100","SE400"} },
                new StudentData { UserId="S02", Name="Omar",  RegisteredCourses=new List<string>{"CPE100","NES200"} },
                new StudentData { UserId="S03", Name="Reem",  RegisteredCourses=new List<string>{"NES200","CIS300","SE400"} },
                new StudentData { UserId="S04", Name="Maher", RegisteredCourses=new List<string>{"CPE100","SE400"} },
            };
            SaveData(StudentsFile, students);

            // loaded data faculty
            var faculty = new List<FacultyData>
            {
                new FacultyData { UserId="F01", Name="Sami", TaughtCourses=new List<string>{"CPE100","CIS300"} },
                new FacultyData { UserId="F02", Name="Eman", TaughtCourses=new List<string>{"NES200","SE400"} },
            };
            SaveData(FacultyFile, faculty);

            // loaded data cards
            var cards = new List<Card>
            {
                new Card { CardNumber="10", Balance=80,  CardType="faculty member", Status="unblocked", UserId="F02" },
                new Card { CardNumber="20", Balance=110, CardType="student",        Status="unblocked", UserId="S02" },
                new Card { CardNumber="30", Balance=95,  CardType="student",        Status="blocked",   UserId="S03" },
                new Card { CardNumber="40", Balance=160, CardType="student",        Status="unblocked", UserId="S04" },
            };
            SaveData(CardsFile, cards);
        }
    }

    // Domain Users
    public abstract class User
    {
        public string UserId { get; }
        public string Name { get; }
        protected Card Card;
        protected SystemManager Manager;

        protected User(string userId, string name, Card card, SystemManager manager)
        {
            UserId = userId;
            Name = name;
            Card = card;
            Manager = manager;
        }

        public bool LoginAllowed() => Card.Status == "unblocked";

        public void RechargeCard()
        {
            Console.WriteLine(Card.GetDetails());

            Console.Write("Enter new balance (recharge amount): ");
            if (!double.TryParse(Console.ReadLine(), out double amount) || amount <= 0)
            {
                Console.WriteLine("Invalid amount.");
                return;
            }

            double old = Card.Balance;
            Card.Balance += amount;
            Manager.SaveCard(Card);

            Console.WriteLine($"Updated Balance = old({old:F2}) + new({amount:F2}) = {Card.Balance:F2} JD");

            Console.Write("Enter transaction ID: ");
            string tid = Console.ReadLine() ?? "";

            Manager.AddTransaction(new Transaction
            {
                TransactionId = tid,
                CardNumber = Card.CardNumber,
                TransactionType = "recharge",
                Amount = amount,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                UserId = UserId
            });
        }
    }

    public class Student : User
    {
        public List<string> RegisteredCourses { get; }

        public Student(string userId, string name, Card card, SystemManager manager, List<string> registeredCourses)
            : base(userId, name, card, manager)
        {
            RegisteredCourses = registeredCourses;
        }

        public void RecordLectureAttendance()
        {
            Console.WriteLine("Your registered courses:");
            foreach (var c in RegisteredCourses) Console.WriteLine("- " + c);

            Console.Write("Enter Course ID: ");
            string courseId = Console.ReadLine() ?? "";

            if (!RegisteredCourses.Contains(courseId))
            {
                Console.WriteLine("Couldn't find the course in your registered courses.");
                return;
            }

            Console.Write("Enter Date (yyyy-MM-dd): ");
            string date = Console.ReadLine() ?? "";

            var record = Manager.GetAttendance()
                .FirstOrDefault(r => r.CourseId == courseId && r.Date == date);

            if (record != null)
            {
                if (!record.AttendeeIds.Contains(UserId))
                    record.AttendeeIds.Add(UserId);
            }
            else
            {
                record = new AttendanceRecord
                {
                    CourseId = courseId,
                    Date = date,
                    AttendeeIds = new List<string> { UserId }
                };
            }

            Manager.SaveAttendance(record);

            Console.Write("Enter transaction ID: ");
            string tid = Console.ReadLine() ?? "";

            Manager.AddTransaction(new Transaction
            {
                TransactionId = tid,
                CardNumber = Card.CardNumber,
                TransactionType = "attendance",
                Amount = null, // N/A
                Date = date,
                UserId = UserId
            });

            Console.WriteLine("Attendance recorded successfully.");
        }

        public void PayForCafeteria()
        {
            // Full menu
            var menu = new Dictionary<int, (string Name, int Price)>
            {
                {1, ("Steak", 8)},
                {2, ("Soup", 2)},
                {3, ("Sandwich", 3)},
                {4, ("Salad", 4)},
                {5, ("Tea", 2)},
                {6, ("Juice", 3)},
                {7, ("Cake", 5)},
                {8, ("Water", 1)},
            };

            Console.WriteLine("[CAFETERIA MENU]");
            foreach (var kv in menu)
                Console.WriteLine($"{kv.Key}:{kv.Value.Name} ({kv.Value.Price} JD)");

            int total = 0;

            Console.Write("Enter an item or 0 to end order: ");
            while (int.TryParse(Console.ReadLine(), out int choice) && choice != 0)
            {
                if (menu.ContainsKey(choice))
                {
                    total += menu[choice].Price;
                }
                else
                {
                    Console.WriteLine("Invalid item.");
                }

                Console.Write("Enter an item or 0 to end order: ");
            }

            if (total == 0)
            {
                Console.WriteLine("No items selected.");
                return;
            }

            if (Card.Balance < total)
            {
                Console.WriteLine("Insufficient card balance!");
                return;
            }

            Card.Balance -= total;
            Manager.SaveCard(Card);
            Console.WriteLine($"Payment Successful! New Balance: {Card.Balance:F2} JD");

            Console.Write("Enter transaction ID: ");
            string tid = Console.ReadLine() ?? "";

            Manager.AddTransaction(new Transaction
            {
                TransactionId = tid,
                CardNumber = Card.CardNumber,
                TransactionType = "payment",
                Amount = total,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                UserId = UserId
            });
        }

        public void PayForBusRide()
        {
            Console.WriteLine("Bus Tracks:");
            Console.WriteLine("1: Track 1 (NB) Northern buildings - 3 JD");
            Console.WriteLine("2: Track 2 (SB) Southern buildings - 4 JD");
            Console.WriteLine("3: Track 3 (LIB) Library - 5 JD");

            Console.Write("Enter track (1/2/3) or shortcut (NB/SB/LIB): ");
            string input = (Console.ReadLine() ?? "").Trim().ToUpper();

            int amount = input switch
            {
                "1" or "NB" => 3,
                "2" or "SB" => 4,
                "3" or "LIB" => 5,
                _ => 0
            };

            if (amount == 0)
            {
                Console.WriteLine("Invalid track.");
                return;
            }

            if (Card.Balance < amount)
            {
                Console.WriteLine("Insufficient card balance!");
                return;
            }

            Card.Balance -= amount;
            Manager.SaveCard(Card);
            Console.WriteLine($"Payment Successful! New Balance: {Card.Balance:F2} JD");

            Console.Write("Enter transaction ID: ");
            string tid = Console.ReadLine() ?? "";

            Manager.AddTransaction(new Transaction
            {
                TransactionId = tid,
                CardNumber = Card.CardNumber,
                TransactionType = "payment",
                Amount = amount,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                UserId = UserId
            });
        }

        public void ViewTransactionHistory()
        {
            var sorted = Manager.GetTransactions()
                .Where(t => t.UserId == UserId)
                .OrderBy(t => t.TransactionType)
                .ToList();

            if (sorted.Count == 0)
            {
                Console.WriteLine("No transactions found.");
                return;
            }

            foreach (var tr in sorted)
                Console.WriteLine(tr.GetDetails());
        }
    }

    public class FacultyMember : User
    {
        public List<string> TaughtCourses { get; }

        public FacultyMember(string userId, string name, Card card, SystemManager manager, List<string> taughtCourses)
            : base(userId, name, card, manager)
        {
            TaughtCourses = taughtCourses;
        }

        public void AccessCarParking()
        {
            Console.WriteLine("Parking Fees:");
            Console.WriteLine("1st hour: 5 JD");
            Console.WriteLine("2nd hour: 4 JD");
            Console.WriteLine("3rd hour: 3 JD");
            Console.WriteLine("4th hour: 2 JD");
            Console.WriteLine("5th hour: 1 JD");
            Console.WriteLine("Above 5 hours: Free");

            Console.Write("Enter number of hours: ");
            if (!int.TryParse(Console.ReadLine(), out int hours) || hours <= 0)
            {
                Console.WriteLine("Invalid hours.");
                return;
            }

            int[] fees = { 5, 4, 3, 2, 1 };
            double cost = 0;

            for (int i = 0; i < hours && i < 5; i++)
                cost += fees[i];

            if (Card.Balance < cost)
            {
                Console.WriteLine("Insufficient card balance!");
                return;
            }

            Card.Balance -= cost;
            Manager.SaveCard(Card);

            Console.WriteLine($"Payment Successful! Cost: {cost:F2} JD | New Balance: {Card.Balance:F2} JD");

            Console.Write("Enter transaction ID: ");
            string tid = Console.ReadLine() ?? "";

            Manager.AddTransaction(new Transaction
            {
                TransactionId = tid,
                CardNumber = Card.CardNumber,
                TransactionType = "payment",
                Amount = cost,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                UserId = UserId
            });
        }

        public void GenerateAttendanceReport()
        {
            Console.WriteLine("Your taught courses:");
            foreach (var c in TaughtCourses) Console.WriteLine("- " + c);

            Console.Write("Enter Course ID: ");
            string courseId = Console.ReadLine() ?? "";
            if (!TaughtCourses.Contains(courseId))
            {
                Console.WriteLine("This course is not in your taught courses.");
                return;
            }

            Console.Write("Enter Date (yyyy-MM-dd): ");
            string date = Console.ReadLine() ?? "";

            var record = Manager.GetAttendance()
                .FirstOrDefault(r => r.CourseId == courseId && r.Date == date);

            if (record == null || record.AttendeeIds.Count == 0)
            {
                Console.WriteLine("Attendees: (none)");
                return;
            }

            Console.WriteLine("Attendees: " + string.Join(", ", record.AttendeeIds));
        }
    }

    public class Administrator
    {
        private SystemManager Manager;

        public Administrator(SystemManager manager)
        {
            Manager = manager;
        }

        public void IssueCard()
        {
            Console.Write("Card Number: ");
            string cn = Console.ReadLine() ?? "";

            Console.Write("Card Type (student / faculty member): ");
            string ct = (Console.ReadLine() ?? "").Trim().ToLower();

            Console.Write("User ID: ");
            string uid = Console.ReadLine() ?? "";

            if (ct == "student")
            {
                var existingStudent = Manager.FindStudent(uid);
                if (existingStudent == null)
                {
                    Console.WriteLine(" Cannot issue card: Student ID not found in students");
                    return;
                }
            }
            else if (ct == "faculty member")
            {
                var existingFaculty = Manager.FindFaculty(uid);
                if (existingFaculty == null)
                {
                    Console.WriteLine("Cannot issue card: Faculty ID not found in faculty");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Invalid Card Type! Use: student OR faculty member");
                return;
            }

            var existingCard = Manager.GetCards().FirstOrDefault(c => c.CardNumber == cn);
            if (existingCard != null)
            {
                Console.WriteLine("Card Number already exists! Choose another number.");
                return;
            }

            // Save the card
            Manager.SaveCard(new Card
            {
                CardNumber = cn,
                CardType = ct,
                Balance = 50,
                Status = "unblocked",
                UserId = uid
            });

            Console.WriteLine("Card Issued Successfully!");
        }

        public void BlockCard()
        {
            var unblocked = Manager.GetCards().Where(c => c.Status == "unblocked").ToList();
            unblocked.ForEach(c => Console.WriteLine(c.GetDetails()));

            Console.Write("Enter Card Number to Block: ");
            string cn = Console.ReadLine() ?? "";

            var card = Manager.GetCards().FirstOrDefault(c => c.CardNumber == cn);
            if (card == null) return;

            card.Status = "blocked";
            Manager.SaveCard(card);
        }

        public void UnblockCard()
        {
            var blocked = Manager.GetCards().Where(c => c.Status == "blocked").ToList();
            blocked.ForEach(c => Console.WriteLine(c.GetDetails()));

            Console.Write("Enter Card Number to Unblock: ");
            string cn = Console.ReadLine() ?? "";

            var card = Manager.GetCards().FirstOrDefault(c => c.CardNumber == cn);
            if (card == null) return;

            card.Status = "unblocked";
            Manager.SaveCard(card);
        }

        public void ViewAllCards()
        {
            Manager.GetCards()
                .OrderBy(c => c.CardType)
                .ToList()
                .ForEach(c => Console.WriteLine(c.GetDetails()));
        }

        public void ViewAllTransactions()
        {
            Manager.GetTransactions()
                .OrderBy(t => t.TransactionType)
                .ToList()
                .ForEach(t => Console.WriteLine(t.GetDetails()));
        }
    }

    // (UI)
    internal class Program
    {
        static void Main(string[] args)
        {
            var manager = new SystemManager();
            var admin = new Administrator(manager);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Smart University Card System ===");
                Console.WriteLine("[1] Login As Admin");
                Console.WriteLine("[2] Login as Card Holder");
                Console.WriteLine("[3] Exit");
                Console.Write("Enter your choice: ");
                string mainChoice = Console.ReadLine() ?? "";

                if (mainChoice == "1") AdminMenu(admin);
                else if (mainChoice == "2") CardHolderLoginScreen(manager);
                else if (mainChoice == "3") break;
            }
        }

        static void AdminMenu(Administrator admin)
        {
            while (true)
            {
                Console.WriteLine("\n--- Administrator Options Menu ---");
                Console.WriteLine("[1] Issue card");
                Console.WriteLine("[2] Block card");
                Console.WriteLine("[3] Unblock card");
                Console.WriteLine("[4] View all cards");
                Console.WriteLine("[5] View all transactions");
                Console.WriteLine("[6] Back To Main Login Screen");
                Console.Write("Choice: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "1") admin.IssueCard();
                else if (choice == "2") admin.BlockCard();
                else if (choice == "3") admin.UnblockCard();
                else if (choice == "4") admin.ViewAllCards();
                else if (choice == "5") admin.ViewAllTransactions();
                else if (choice == "6") break;
            }
        }

        static void CardHolderLoginScreen(SystemManager manager)
        {
            while (true)
            {
                Console.WriteLine("\n--- Card Holders’ Login Screen ---");
                Console.WriteLine("[1] Login As Student");
                Console.WriteLine("[2] Login as Faculty Member");
                Console.WriteLine("[3] Back To Main Login Screen");
                Console.Write("Choice: ");
                string typeChoice = Console.ReadLine() ?? "";

                if (typeChoice == "3") break;

                Console.Write("Enter Card Number: ");
                string cardNum = Console.ReadLine() ?? "";

                var card = manager.GetCards().FirstOrDefault(c => c.CardNumber == cardNum);

                if (card == null)
                {
                    Console.WriteLine("Invalid card number!");
                    continue;
                }
                if (card.Status == "blocked")
                {
                    Console.WriteLine("This card is blocked!");
                    continue;
                }

                if (typeChoice == "1" && card.CardType == "student")
                {
                    var sd = manager.FindStudent(card.UserId);
                    if (sd == null) { Console.WriteLine("Student data not found!"); continue; }

                    var stdUser = new Student(sd.UserId, sd.Name, card, manager, sd.RegisteredCourses);
                    StudentMenu(stdUser);
                }
                else if (typeChoice == "2" && card.CardType == "faculty member")
                {
                    var fd = manager.FindFaculty(card.UserId);
                    if (fd == null) { Console.WriteLine("Faculty data not found!"); continue; }

                    var facUser = new FacultyMember(fd.UserId, fd.Name, card, manager, fd.TaughtCourses);
                    FacultyMenu(facUser);
                }
                else
                {
                    Console.WriteLine("Card type mismatch!");
                }
            }
        }

        static void StudentMenu(Student student)
        {
            while (true)
            {
                Console.WriteLine($"\n--- Welcome Student {student.Name} ({student.UserId}) ---");
                Console.WriteLine("[1] Recharge card");
                Console.WriteLine("[2] Record lecture attendance");
                Console.WriteLine("[3] Pay for cafeteria");
                Console.WriteLine("[4] Pay for bus ride");
                Console.WriteLine("[5] View transaction history");
                Console.WriteLine("[6] Logout");
                Console.Write("Choice: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "1") student.RechargeCard();
                else if (choice == "2") student.RecordLectureAttendance();
                else if (choice == "3") student.PayForCafeteria();
                else if (choice == "4") student.PayForBusRide();
                else if (choice == "5") student.ViewTransactionHistory();
                else if (choice == "6") break;
            }
        }

        static void FacultyMenu(FacultyMember faculty)
        {
            while (true)
            {
                Console.WriteLine($"\n--- Welcome Faculty Member {faculty.Name} ({faculty.UserId}) ---");
                Console.WriteLine("[1] Recharge card");
                Console.WriteLine("[2] Access car parking");
                Console.WriteLine("[3] Generate attendance report");
                Console.WriteLine("[4] Logout");
                Console.Write("Choice: ");
                string choice = Console.ReadLine() ?? "";

                if (choice == "1") faculty.RechargeCard();
                else if (choice == "2") faculty.AccessCarParking();
                else if (choice == "3") faculty.GenerateAttendanceReport();
                else if (choice == "4") break;
            }
        }
    }
}
