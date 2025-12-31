using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

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
}   