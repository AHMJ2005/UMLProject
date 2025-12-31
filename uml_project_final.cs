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
            Balance = 20,
            Status = "unblocked",
            UserId = userId
        });
    }

    public void BlockCard(string cardNumber)
    {
        var card = Manager.GetCards().First(c => c.CardNumber == cardNumber);
        card.Status = "blocked";
        Manager.SaveCard(card);
    }

    public void UnblockCard(string cardNumber)
    {
        var card = Manager.GetCards().First(c => c.CardNumber == cardNumber);
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
   