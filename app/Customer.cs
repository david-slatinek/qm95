using System;
using System.Linq;
using System.Data.SQLite;
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
        private static readonly string ConnectionString = "Data Source=db;Cache=Shared;";

        public Customer(string name, string lastName, string email, string password)
        {
            Name = EditNameOrLastname(name);
            LastName = EditNameOrLastname(lastName);
            Email = email.TrimStart().TrimEnd();
            GeneratePasswordHash(password);
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

        private void GeneratePasswordHash(string password)
        {
            byte[] salt = new byte[32];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            Salt = Convert.ToBase64String(salt);

            using (Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                Password = Convert.ToBase64String(rfc.GetBytes(32));
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
    }
}