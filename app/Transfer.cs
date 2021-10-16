using System;
using System.Data.SQLite;
using System.Collections.Generic;

namespace qm95
{
    public class Transfer : ITransfer
    {
        public int IdTransfer { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime TransferDate { get; private set; }
        public TransferType Type { get; private set; }

        private Transfer(int idTransfer, decimal amount, DateTime transferDate, TransferType type)
        {
            IdTransfer = idTransfer;
            Amount = amount;
            TransferDate = transferDate;
            Type = type;
        }

        public Transfer(decimal amount, TransferType type)
        {
            Amount = amount;
            TransferDate = DateTime.Today;
            Type = type;
        }

        public static List<Transfer> GetTransfers(int idAccount)
        {
            var transfers = new List<Transfer>();
            try
            {
                using var connection = new SQLiteConnection(Customer.ConnectionString);
                using var cm =
                    new SQLiteCommand(
                        "SELECT transfer.id_transfer, transfer.amount, " +
                        "transfer.transfer_date, transfer_type.description " + "FROM transfer " +
                        "JOIN transfer_type " + "ON transfer.fk_transfer_type = transfer_type.id_transfer_type " +
                        "WHERE transfer.fk_account = @fk_account;",
                        connection);

                cm.Parameters.AddWithValue("@fk_account", idAccount);

                connection.Open();

                using var reader = cm.ExecuteReader();

                while (reader.Read())
                {
                    int idTransfer = Convert.ToInt32(reader["id_transfer"]);
                    decimal amount = (decimal) reader["amount"];
                    DateTime transferDate = Convert.ToDateTime(reader["transfer_date"]);
                    transfers.Add(new Transfer(idTransfer, amount, transferDate,
                        (TransferType) Enum.Parse(typeof(TransferType), (string) reader["description"])));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return transfers;
        }

        public string MakeTransfer(int idAccount)
        {
            try
            {
                using var connection = new SQLiteConnection(Customer.ConnectionString);
                using var cm =
                    new SQLiteCommand("INSERT INTO transfer (amount, transfer_date, fk_account, fk_transfer_type) " +
                                      "VALUES (@amount, @transfer_date, @fk_account, @fk_transfer_type) RETURNING id_transfer;",
                        connection);
                cm.Parameters.AddWithValue("@amount", Amount);
                cm.Parameters.AddWithValue("@transfer_date", TransferDate.ToString("yyyy-MM-dd"));
                cm.Parameters.AddWithValue("@fk_account", idAccount);
                cm.Parameters.AddWithValue("@fk_transfer_type", (int) Type);

                connection.Open();
                IdTransfer = Convert.ToInt32(cm.ExecuteScalar());
                return null;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public string RevertTransaction()
        {
            try
            {
                using var connection = new SQLiteConnection(Customer.ConnectionString);
                using var cm = new SQLiteCommand("DELETE FROM transfer WHERE id_transfer = @id_transfer;", connection);

                cm.Parameters.AddWithValue("@id_transfer", IdTransfer);

                connection.Open();
                cm.ExecuteScalar();
                return null;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}