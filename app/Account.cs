using System;
using System.Data.SQLite;

namespace qm95
{
    public class Account
    {
        public int IdAccount { get; private set; }
        public DateTime OpeningDate { get; private set; }
        public decimal Balance { get; private set; }
        public AccountType Type { get; private set; }

        public Account(int idAccount, DateTime openingDate, decimal balance, AccountType type)
        {
            IdAccount = idAccount;
            OpeningDate = openingDate;
            Balance = balance;
            Type = type;
        }

        public static Account CreateAccount(AccountType type, int idCustomer)
        {
            try
            {
                using (var connection = new SQLiteConnection(Customer.ConnectionString))
                using (var cm =
                    new SQLiteCommand(
                        "INSERT INTO account (opening_date, balance, fk_account_type, fk_customer) VALUES (@opening_date, 0.0, @fk_account_type, @fk_customer) RETURNING id_account;",
                        connection))
                {
                    cm.Parameters.AddWithValue("@opening_date", DateTime.Now.ToString("yyyy-MM-dd"));
                    Console.WriteLine((int) type);
                    cm.Parameters.AddWithValue("@fk_account_type", (int) type);
                    cm.Parameters.AddWithValue("@fk_customer", idCustomer);

                    connection.Open();
                    int id = Convert.ToInt32(cm.ExecuteScalar());
                    return new Account(id, DateTime.Now, (decimal) 0.0, type);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}