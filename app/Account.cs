using System;
using System.Data.SQLite;
using System.Collections.Generic;

namespace qm95
{
    public class Account
    {
        public int IdAccount { get; private set; }
        public DateTime OpeningDate { get; private set; }
        public decimal Balance { get; private set; }
        public AccountType Type { get; private set; }
        public List<Transfer> Transfers { get; private set; }

        public Account(int idAccount, DateTime openingDate, decimal balance, AccountType type)
        {
            IdAccount = idAccount;
            OpeningDate = openingDate;
            Balance = balance;
            Type = type;
            Transfers = Transfer.GetTransfers(IdAccount);
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

        public string CloseAccount(int idCustomer)
        {
            try
            {
                using var connection = new SQLiteConnection(Customer.ConnectionString);
                using var cm = new SQLiteCommand(
                    "UPDATE account SET closing_date = @closing_date WHERE fk_account_type = @fk_account_type "
                    + "AND fk_customer = @fk_customer;", connection);

                cm.Parameters.AddWithValue("@closing_date", DateTime.Now.ToString("yyyy-MM-dd"));
                cm.Parameters.AddWithValue("@fk_account_type", (int) Type);
                cm.Parameters.AddWithValue("@fk_customer", idCustomer);

                connection.Open();
                cm.ExecuteNonQuery();
                return null;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private string UpdateBalance(decimal amount, TransferType type)
        {
            if (type == TransferType.Withdraw) amount *= -1;
            Balance += amount;

            try
            {
                using var connection = new SQLiteConnection(Customer.ConnectionString);
                using var cm =
                    new SQLiteCommand("UPDATE account SET balance = @balance WHERE fk_customer = @fk_customer;",
                        connection);
                cm.Parameters.AddWithValue("@balance", Balance);
                cm.Parameters.AddWithValue("@fk_customer", IdAccount);

                connection.Open();
                cm.ExecuteScalar();
                return null;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public string MakeTransfer(decimal amount, TransferType type)
        {
            Transfer transfer = new Transfer(amount, type);
            string result = transfer.MakeTransfer(IdAccount);
            if (result is not null) return result;

            result = UpdateBalance(amount, type);
            if (result is not null) transfer.RevertTransaction();

            return result;
        }
    }
}