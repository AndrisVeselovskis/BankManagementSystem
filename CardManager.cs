using System;
using System.Collections.Generic;
using System.Data.SQLite;
using BankManagementSystem.Models;

namespace BankManagementSystem.Database
{
    public class CardManager
    {
        private string connectionString = "Data Source=bank.db;Version=3;";

        public void AddCard(Card card)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Cards (AccountNumber, CardType, IsActive, Terms, CreditLimit, OverdraftLimit) " +
                               "VALUES (@AccountNumber, @CardType, @IsActive, @Terms, @CreditLimit, @OverdraftLimit)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AccountNumber", card.AccountNumber);
                    command.Parameters.AddWithValue("@CardType", card.GetType().Name);
                    command.Parameters.AddWithValue("@IsActive", card.IsActive);
                    command.Parameters.AddWithValue("@Terms", card.Terms ?? "");

                    if (card is CreditCard creditCard)
                        command.Parameters.AddWithValue("@CreditLimit", creditCard.CreditLimit);
                    else
                        command.Parameters.AddWithValue("@CreditLimit", DBNull.Value);

                    if (card is DebitCard debitCard)
                        command.Parameters.AddWithValue("@OverdraftLimit", debitCard.OverdraftLimit);
                    else
                        command.Parameters.AddWithValue("@OverdraftLimit", DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Card> LoadCards(string accountNumber)
        {
            List<Card> cards = new List<Card>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Cards WHERE AccountNumber = @AccountNumber";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string cardType = reader["CardType"].ToString();
                            bool isActive = Convert.ToBoolean(reader["IsActive"]);
                            string terms = reader["Terms"].ToString();

                            Card card;
                            if (cardType == nameof(CreditCard))
                            {
                                card = new CreditCard
                                {
                                    CreditLimit = Convert.ToDecimal(reader["CreditLimit"])
                                };
                            }
                            else if (cardType == nameof(DebitCard))
                            {
                                card = new DebitCard
                                {
                                    OverdraftLimit = Convert.ToDecimal(reader["OverdraftLimit"])
                                };
                            }
                            else
                            {
                                card = new Card();
                            }

                            card.CardID = Convert.ToInt32(reader["CardID"]);
                            card.AccountNumber = accountNumber;
                            card.IsActive = isActive;
                            card.Terms = terms;

                            cards.Add(card);
                        }
                    }
                }
            }
            return cards;
        }

        public void SetCardStatus(int cardID, bool isActive)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Cards SET IsActive = @IsActive WHERE CardID = @CardID";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    command.Parameters.AddWithValue("@CardID", cardID);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
