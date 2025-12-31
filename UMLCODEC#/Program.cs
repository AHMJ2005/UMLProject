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

        internal class Program
        {
            static void Main(string[] args)
            {

            }
        }
    }
}
