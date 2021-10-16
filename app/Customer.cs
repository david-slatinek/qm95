using System;
using System.Linq;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace qm95
{
    public class Customer
    {
        public int IdCustomer { get; private set; }
        public string Name { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public string Salt { get; private set; }
        public List<Account> Accounts { get; private set; }

        public static readonly string ConnectionString = "Data Source=db;Cache=Shared;";
        private static readonly int Iterations = 10000;
        private static readonly int Size = 32;

        public Customer(string name, string lastName, string email, string password)
        {
            Name = EditNameOrLastname(name);
            LastName = EditNameOrLastname(lastName);
            Email = email.TrimStart().TrimEnd();

            byte[] salt = GenerateSalt();
            Salt = Convert.ToBase64String(salt);
            Password = Convert.ToBase64String(GenerateHash(password, salt));
        }

        public Customer(string email, string password)
        {
            Email = email;
            Password = password;
        }

        public Customer()
        {
        }

        private static string EditNameOrLastname(string s)
        {
            s = s.TrimStart().TrimEnd();
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsDigit(s[i]) || char.IsPunctuation(s[i]) || char.IsSeparator(s[i]) || char.IsWhiteSpace(s[i]))
                {
                    s = s.Remove(i);
                }
            }

            return s.Length >= 2 ? char.ToUpper(s[0]) + s.Substring(1) : char.ToUpper(s[0]).ToString();
        }

        public static bool IsValidPassword(string password)
        {
            if (password.Length < 10) return false;
            return password.Any(c => char.IsUpper(c)) && password.Any(c => char.IsNumber(c)) &&
                   password.Any(c => char.IsPunctuation(c));
        }

        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[Size];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }

        private byte[] GenerateHash(string password, byte[] salt) =>
            new Rfc2898DeriveBytes(password, salt, Iterations).GetBytes(Size);

        public static bool IsValidEmail(string email)
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                using (var cm = new SQLiteCommand("SELECT 1 FROM customer WHERE email = @email;", connection))
                {
                    cm.Parameters.AddWithValue("@email", email);
                    connection.Open();
                    return cm.ExecuteScalar() is null;
                }
            }
            catch
            {
                return false;
            }
        }

        public string SaveCustomer()
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                using (var cm =
                    new SQLiteCommand(
                        "INSERT INTO customer (name, lastname, email, password, salt) VALUES (@name, @lastname, @email, @password, @salt) RETURNING id_customer;",
                        connection))
                {
                    cm.Parameters.AddWithValue("@name", Name);
                    cm.Parameters.AddWithValue("@lastname", LastName);
                    cm.Parameters.AddWithValue("@email", Email);
                    cm.Parameters.AddWithValue("@password", Password);
                    cm.Parameters.AddWithValue("@salt", Salt);

                    connection.Open();
                    IdCustomer = Convert.ToInt32(cm.ExecuteScalar());
                    return null;
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public bool Login()
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                using (var cm =
                    new SQLiteCommand(
                        "SELECT id_customer, name, lastname, password, salt FROM customer WHERE email = @email;",
                        connection))
                {
                    cm.Parameters.AddWithValue("@email", Email);

                    connection.Open();

                    using (var reader = cm.ExecuteReader())
                    {
                        if (!reader.Read()) return false;

                        string password = (string) reader["password"];
                        Salt = (string) reader["salt"];

                        if (Convert.ToBase64String(GenerateHash(Password, Convert.FromBase64String(Salt))) != password)
                            return false;
                        Password = password;

                        IdCustomer = Convert.ToInt32(reader["id_customer"]);
                        Name = (string) reader["name"];
                        LastName = (string) reader["lastname"];
                    }

                    GetAllAccounts();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void GetAllAccounts()
        {
            Accounts = new List<Account>();
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                using (var cm =
                    new SQLiteCommand(
                        "SELECT account.id_account, account.opening_date, account.balance, account_type.description " +
                        "FROM account JOIN account_type ON account.fk_account_type = account_type.id_account_type " +
                        "WHERE account.fk_customer = @fk_customer AND account.closing_date IS NULL;",
                        connection))
                {
                    cm.Parameters.AddWithValue("@fk_customer", IdCustomer);

                    connection.Open();

                    using (var reader = cm.ExecuteReader())
                    {
                        if (!reader.HasRows) return;

                        while (reader.Read())
                        {
                            int idAccount = Convert.ToInt32(reader["id_account"]);
                            DateTime openingDate = Convert.ToDateTime(reader["opening_date"]);
                            decimal balance = (decimal) reader["balance"];
                            Accounts.Add(new Account(idAccount, openingDate, balance,
                                (AccountType) Enum.Parse(typeof(AccountType), (string) reader["description"])));
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private bool AccountExists(AccountType type) => Accounts.Any(a => a.Type == type);

        public string CreateAccount(AccountType type)
        {
            if (AccountExists(type)) return "Account type already exists!";
            Account account = Account.CreateAccount(type, IdCustomer);
            if (account is null) return "Error while creating account";
            Accounts.Add(account);
            return null;
        }

        public string CloseAccount(int index)
        {
            string result = Accounts[index].CloseAccount(IdCustomer);
            if (result is null) Accounts.RemoveAt(index);
            return result;
        }
    }
}