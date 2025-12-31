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
    class systemManger
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
            if(!File.Exists(file))
            {
                File.WriteAllText(file, "[]");
            }
        }
        private List<T> LoadData<T>(string file)
        {
            string Fjson = File.ReadAllText(file);
            return JsonSerializer.Deserialize<List<T>>(Fjson) ?? new List<T>();
        }
        private void SaveData<T>(string file , List<T> data)
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

    internal class Program
    {
        static void Main(string[] args)
        {

        }
    }
}
