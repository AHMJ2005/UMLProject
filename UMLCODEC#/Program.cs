using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
namespace UMLCODEC_
{

    public class Card
    {
        public string CardNumber { get; set; }
        public string CardType { get; set; } // Student or Faculty
        public double Balance { get; set; }
        public string Status { get; set; } // unblocked or blocked
        public string UserId { get; set; }
        public string GetDetails() =>
            $"Card #{CardNumber} | Type: {CardType} | Balance: {Balance:F2} JD | Status: {Status} | User: {UserId}";
    }
    public class Transaction
    {

        public string TransactionId { get; set; }
        public string TransactionType { get; set; }
        public double Amount { get; set; }
        public string Date { get; set; }
        public string UserId { get; set; }

        public string GetDetails() =>
            $"ID: {TransactionId} | Type: {TransactionType} | Amount: {Amount} | Date: {Date} | User: {UserId}";
    }
    public class AttendanceRecord
    {
        public string CourseId { get; set; }
        public string Date { get; set; }
        public List<string> AttendeeIds { get; set; } = new List<string>();
    }
    public class systemManger
    {
        private const string CardsFile = "cards.json";
        private const string TransactionsFile = "transactions.json";
        private const string AttendanceFile = "attendance.json";
        public systemManger()
        {
            InitializeFile(CardsFile);
            InitializeFile(TransactionsFile);
            InitializeFile(AttendanceFile);
        }
        private void InitializeFile(string file)
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, "[]");
            }
        }
        private List<T> LoadData<T>(string file)
        {
            string Fjson = File.ReadAllText(file);
            return JsonSerializer.Deserialize<List<T>>(Fjson) ?? new List<T>();
        }
        private void SaveData<T>(string file, List<T> data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(file, json);
        }
        public List<Card> GetCards() => LoadData<Card>(CardsFile);
        public void SaveCard(Card card)
        {
            var cards = GetCards();
            var index = cards.FindIndex(c => c.CardNumber == card.CardNumber);

            if (index != -1) cards[index] = card;
            else cards.Add(card);

            SaveData(CardsFile, cards);
        }
        public List<Transaction> GetTransactions() => LoadData<Transaction>(TransactionsFile);
        public void AddTransaction(Transaction tr)
        {
            var trs = GetTransactions();
            trs.Add(tr);
            SaveData(TransactionsFile, trs);
        }
        public List<AttendanceRecord> GetAttendance() => LoadData<AttendanceRecord>(AttendanceFile);
        public void SaveAttendance(AttendanceRecord record)
        {
            var records = GetAttendance();
            var index = records.FindIndex(r => r.CourseId == record.CourseId && r.Date == record.Date);

            if (index != -1)
                records[index] = record;
            else
                records.Add(record);

            SaveData(AttendanceFile, records);
        }

    }
    public abstract class User
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        protected Card Card;
        protected systemManger Manager;

        protected User(string userId, string name, Card card, systemManger manager)
        {
            UserId = userId;
            Name = name;
            Card = card;
            Manager = manager;
        }

        public bool Login()
        {
            return Card.Status == "unblocked";
        }

        public void RechargeCard(double amount, string transactionId)
        {
            if (amount <= 0) return;

            Card.Balance += amount;
            Manager.SaveCard(Card);

            Manager.AddTransaction(new Transaction
            {
                TransactionId = transactionId,
                TransactionType = "Recharge",
                Amount = amount,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                UserId = UserId
            });
        }
    }


    public class Administrator
    {
        private systemManger Manager;

        public Administrator(systemManger manager)
        {
            Manager = manager;
        }

        public void IssueCard(string cardNumber, string cardType, string userId)
        {
            Manager.SaveCard(new Card
            {
                CardNumber = cardNumber,
                CardType = cardType,
                Balance = 50,
                Status = "unblocked",
                UserId = userId
            });
        }

        public void BlockCard(string cardNumber)
        {
            var card = Manager.GetCards().FirstOrDefault(c => c.CardNumber == cardNumber);
            if (card == null) return;

            card.Status = "blocked";
            Manager.SaveCard(card);
        }

        public void UnblockCard(string cardNumber)
        {
            var card = Manager.GetCards().FirstOrDefault(c => c.CardNumber == cardNumber);
            if (card == null) return;

            card.Status = "unblocked";
            Manager.SaveCard(card);
        }

        public List<Card> ViewAllCards()
        {
            return Manager.GetCards().OrderBy(c => c.CardType).ToList();
        }

        public List<Transaction> ViewAllTransactions()
        {
            return Manager.GetTransactions().OrderBy(t => t.TransactionType).ToList();
        }
    }


    public class FacultyMember : User
    {
        public List<string> TaughtCourses { get; set; } = new();

        public FacultyMember(string userId, string name, Card card, systemManger manager)
            : base(userId, name, card, manager)
        {
        }

        public void AccessCarParking(int hours, string transactionId)
        {
            if (hours <= 0) return;

            int[] fees = { 5, 4, 3, 2, 1 };
            double cost = 0;

            for (int i = 0; i < hours && i < 5; i++)
                cost += fees[i];

            if (Card.Balance < cost) return;

            Card.Balance -= cost;
            Manager.SaveCard(Card);

            Manager.AddTransaction(new Transaction
            {
                TransactionId = transactionId,
                TransactionType = "Payment",
                Amount = cost,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                UserId = UserId
            });
        }

        public List<string> GenerateAttendanceReport(string courseId, string date)
        {
            if (!TaughtCourses.Contains(courseId))
                return new List<string>();

            var record = Manager.GetAttendance()
                .FirstOrDefault(r => r.CourseId == courseId && r.Date == date);

            return record == null ? new List<string>() : record.AttendeeIds;
        }
        public class Student : User
        {
            public Student(string userId, string name, Card card, systemManger manager)
              : base(userId, name, card, manager) { }
            public List<string> RegisteredCourses { get; set; } = new List<string>();

            public void RecordLectureAttendance(string CourseID, string date)
            {
                foreach (string RegCourse in RegisteredCourses)
                    Console.WriteLine("-" + RegCourse);

                if (!RegisteredCourses.Contains(CourseID))
                    Console.WriteLine("Couldn't find the course in your Registerd Courses");

                List<AttendanceRecord> allRecords = Manager.GetAttendance();
                AttendanceRecord record = allRecords.FirstOrDefault(r => r.CourseId == CourseID && r.Date == date);

                if (record != null)
                {
                    if (!record.AttendeeIds.Contains(UserId)) //just to prevent the duplication
                        record.AttendeeIds.Add(UserId);
                }
                else
                {
                    record = new AttendanceRecord();
                    record.CourseId = CourseID;
                    record.Date = date;
                    record.AttendeeIds = new List<string>();
                    record.AttendeeIds.Add(UserId);
                }
                Manager.SaveAttendance(record);

                Transaction RcrdTransaction = new Transaction();
                Console.WriteLine("enter transactionId for the Lecture Record");
                string transactionId = Console.ReadLine();
                RcrdTransaction.TransactionId = transactionId;
                RcrdTransaction.TransactionType = "attendance";
                RcrdTransaction.Amount = 0;
                RcrdTransaction.Date = date;
                RcrdTransaction.UserId = this.UserId;
                Manager.AddTransaction(RcrdTransaction);

            }
            public void PayforCafteria(string date)
            {
                Console.WriteLine("1:Steak (8 JD)    2:Soup(2 JD)    3:Sandwich(3 JD)");
                int SteakCtr = 0;
                int SoupCtr = 0;
                int SandwichCtr = 0;

                Console.WriteLine("enter your choice from the menu or 0 to end order");
                int MenuChoice = Convert.ToInt32(Console.ReadLine());
                if (MenuChoice == 1)
                    SteakCtr++;
                else if (MenuChoice == 2)
                    SoupCtr++;
                else if (MenuChoice == 3)
                    SandwichCtr++;
                while (MenuChoice != 0)
                {
                    Console.WriteLine("enter your choice from the menu or 0 to end order");
                    MenuChoice = Convert.ToInt32(Console.ReadLine());
                    if (MenuChoice == 1)
                        SteakCtr++;
                    else if (MenuChoice == 2)
                        SoupCtr++;
                    else if (MenuChoice == 3)
                        SandwichCtr++;
                }
                int amount = SteakCtr * 8 + SoupCtr * 2 + SandwichCtr * 3;
                if (Card.Balance >= amount)
                {
                    Card.Balance -= amount;
                    Manager.SaveCard(Card);
                    Console.WriteLine($"Payment Successful! New Balance: {Card.Balance} JD");
                }
                else
                {
                    Console.WriteLine("Insufficient card balance!");
                }
                Transaction CftriaTransaction = new Transaction();
                Console.WriteLine("enter Transaction ID");
                string transactionID = Console.ReadLine();
                CftriaTransaction.TransactionId = transactionID;
                CftriaTransaction.TransactionType = "payment";
                CftriaTransaction.Amount = amount;
                CftriaTransaction.Date = date;
                CftriaTransaction.UserId = this.UserId;

                Manager.AddTransaction(CftriaTransaction);


            }

            public void PayforBus(string date)

            {
                Console.WriteLine("NB: North Buildings    SB: South Buildings");
                Console.WriteLine("enter a destination: ");
                string dest = Console.ReadLine();
                int amount = 0;
                if (dest == "NB")
                    amount = 3;
                else if (dest == "SB")
                    amount = 4;
                if (Card.Balance >= amount)
                {
                    Card.Balance -= amount; 
                    Manager.SaveCard(Card);
                    Console.WriteLine($"Payment Successful! New Balance: {Card.Balance} JD");
                }
                else
                {
                    Console.WriteLine("Insufficient card balance!");
                }
                Transaction BusTransaction = new Transaction();
                Console.WriteLine("enter Transaction ID");
                string transactionId = Console.ReadLine();
                BusTransaction.Amount = amount;
                BusTransaction.TransactionType = "payment";
                BusTransaction.TransactionId = transactionId;
                BusTransaction.Date = date;
                BusTransaction.UserId = this.UserId;

                Manager.AddTransaction(BusTransaction);
            }

            public void ViewTransactionHistory()
            {
                List<Transaction> AllTransactions = Manager.GetTransactions();
                List<Transaction> StudentTr = new List<Transaction>();
                foreach (Transaction tr in AllTransactions)
                {
                    if (tr.UserId == this.UserId)
                        StudentTr.Add(tr);
                }
                List<Transaction> SortedStudentTr = StudentTr.OrderBy(t => t.TransactionType).ToList();

                foreach (Transaction tr in SortedStudentTr)
                {
                    Console.WriteLine(tr.GetDetails());
                }
            }
        }

        internal class Program
        {
            static void Main(string[] args)
            {
                systemManger manager = new systemManger();
                Administrator admin = new Administrator(manager);

                while (true)
                {
                    // [Main Login Screen]
                    Console.Clear();
                    Console.WriteLine("=== Smart University Card System ===");
                    Console.WriteLine("[1] Login As Admin");
                    Console.WriteLine("[2] Login as Card Holder");
                    Console.WriteLine("[3] Exit");
                    Console.Write("Enter your choice: ");
                    string mainChoice = Console.ReadLine();

                    if (mainChoice == "1")
                    {
                        AdminMenu(admin, manager);
                    }
                    else if (mainChoice == "2")
                    {
                        CardHolderLoginScreen(manager);
                    }
                    else if (mainChoice == "3")
                    {
                        break;
                    }
                }


            }
            static void AdminMenu(Administrator admin, systemManger manager)
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
                    string choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        Console.Write("Card Number: "); string cn = Console.ReadLine();
                        Console.Write("Type (Student/Faculty): "); string ct = Console.ReadLine();
                        Console.Write("User ID: "); string uid = Console.ReadLine();
                        admin.IssueCard(cn, ct, uid);
                        Console.WriteLine("Card Issued Successfully!");
                    }
                    else if (choice == "2")
                    {
                        //  Use Case 10 "unblock"
                        var unblocked = manager.GetCards().Where(c => c.Status == "unblocked").ToList();
                        unblocked.ForEach(c => Console.WriteLine(c.GetDetails()));
                        Console.Write("Enter Card Number to Block: ");
                        admin.BlockCard(Console.ReadLine());
                    }
                    else if (choice == "3")
                    {
                        // Use Case 11 "block"
                        var blocked = manager.GetCards().Where(c => c.Status == "blocked").ToList();
                        blocked.ForEach(c => Console.WriteLine(c.GetDetails()));
                        Console.Write("Enter Card Number to Unblock: ");
                        admin.UnblockCard(Console.ReadLine());
                    }
                    else if (choice == "4")
                    {
                        admin.ViewAllCards().ForEach(c => Console.WriteLine(c.GetDetails()));
                    }
                    else if (choice == "5")
                    {
                        admin.ViewAllTransactions().ForEach(t => Console.WriteLine(t.GetDetails()));
                    }
                    else if (choice == "6") break;
                }
            }

            // cardHolder login 
            static void CardHolderLoginScreen(systemManger manager)
            {
                while (true)
                {
                    Console.WriteLine("\n--- Card Holders' Login Screen ---");
                    Console.WriteLine("[1] Login As Student");
                    Console.WriteLine("[2] Login as Faculty Member");
                    Console.WriteLine("[3] Back To Main Login Screen");
                    Console.Write("Choice: ");
                    string typeChoice = Console.ReadLine();

                    if (typeChoice == "3") break;

                    Console.Write("Enter Card Number: ");
                    string cardNum = Console.ReadLine();
                    var card = manager.GetCards().FirstOrDefault(c => c.CardNumber == cardNum);

                    //   (Limitations) UI Guidelines Page 3
                    if (card == null) { Console.WriteLine("Invalid card number!"); continue; }
                    if (card.Status == "blocked") { Console.WriteLine("This card is blocked!"); continue; }

                    if (typeChoice == "1" && card.CardType == "Student")
                    {
                        Student stdUser = new Student(card.UserId, "Student Name", card, manager);
                        StudentMenu(stdUser);
                    }
                    else if (typeChoice == "2" && card.CardType == "Faculty")
                    {
                        FacultyMember facUser = new FacultyMember(card.UserId, "Faculty Name", card, manager);
                        FacultyMenu(facUser);
                    }
                    else { Console.WriteLine("Card type mismatch!"); }
                }
            }

            
            static void StudentMenu(Student student)
            {
                while (true)
                {
                    Console.WriteLine($"\n--- Welcome Student {student.UserId} ---");
                    Console.WriteLine("[1] Recharge card");
                    Console.WriteLine("[2] Record lecture attendance");
                    Console.WriteLine("[3] Pay for cafeteria");
                    Console.WriteLine("[4] Pay for bus ride");
                    Console.WriteLine("[5] View transaction history");
                    Console.WriteLine("[6] Logout");
                    Console.Write("Choice: ");
                    string choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        Console.Write("Enter Amount: "); double amt = Convert.ToDouble(Console.ReadLine());
                        Console.Write("Enter Transaction ID: "); string tid = Console.ReadLine();
                        student.RechargeCard(amt, tid);
                    }
                    else if (choice == "2")
                    {
                        Console.Write("Enter Course ID: "); string cid = Console.ReadLine();
                        Console.Write("Enter Date (yyyy-MM-dd): "); string dt = Console.ReadLine();
                        student.RecordLectureAttendance(cid, dt);
                    }
                    else if (choice == "3") student.PayforCafteria(DateTime.Now.ToString("yyyy-MM-dd"));
                    else if (choice == "4") student.PayforBus(DateTime.Now.ToString("yyyy-MM-dd"));
                    else if (choice == "5") student.ViewTransactionHistory();
                    else if (choice == "6") break;
                }
            }

            
            static void FacultyMenu(FacultyMember faculty)
            {
                while (true)
                {
                    Console.WriteLine($"\n--- Welcome Faculty Member {faculty.UserId} ---");
                    Console.WriteLine("[1] Recharge card");
                    Console.WriteLine("[2] Access car parking");
                    Console.WriteLine("[3] Generate attendance report");
                    Console.WriteLine("[4] Logout");
                    Console.Write("Choice: ");
                    string choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        Console.Write("Amount: "); double amt = Convert.ToDouble(Console.ReadLine());
                        Console.Write("T-ID: "); string tid = Console.ReadLine();
                        faculty.RechargeCard(amt, tid);
                    }
                    else if (choice == "2")
                    {
                        Console.Write("Hours: "); int h = Convert.ToInt32(Console.ReadLine());
                        Console.Write("T-ID: "); string tid = Console.ReadLine();
                        faculty.AccessCarParking(h, tid);
                    }
                    else if (choice == "3")
                    {
                        Console.Write("Course ID: "); string cid = Console.ReadLine();
                        Console.Write("Date: "); string dt = Console.ReadLine();
                        var report = faculty.GenerateAttendanceReport(cid, dt);
                        Console.WriteLine("Attendees: " + string.Join(", ", report));
                    }
                    else if (choice == "4") break;
                }
            }
        }
    }
}
