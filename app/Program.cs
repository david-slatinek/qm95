using System;
using static System.Console;

namespace qm95
{
    class Program
    {
        private static void Help()
        {
            WriteLine("Name");
            WriteLine("\tA private banking system");

            WriteLine("Flags");
            WriteLine("-h");
            WriteLine("\tPrint help");
            WriteLine("-L");
            WriteLine("\tLogin");
            WriteLine("-r");
            WriteLine("\tRegister");
        }

        private static Customer Login()
        {
            Write("Enter email: ");
            string email = ReadLine();
            
            Write("Enter password: ");
            string password = ReadLine();

            Customer customer = new Customer(email, password);
            return customer.Login() ? customer : null;
        }

        private static string Register()
        {
            Write("Your name: ");
            string name = ReadLine();
            Write("Your lastname: ");
            string lastName = ReadLine();

            string email;
            while (true)
            {
                Write("Enter email: ");
                email = ReadLine();

                if (!Customer.IsValidEmail(email)) WriteLine("Email already exists, ty again.");
                else break;
            }

            string password;
            while (true)
            {
                Write("Enter password (minimum: 10 characters, 1 uppercase letter, 1 number, 1 punctuation): ");
                password = ReadLine();

                if (!Customer.IsValidPassword(password)) WriteLine("Invalid password, ty again.");
                else break;
            }

            return new Customer(name, lastName, email, password).SaveCustomer();
        }

        private static void Main()
        {
            Title = "qm95";

            int option = -1;
            while (option < 1 || option > 3)
            {
                WriteLine("Choose an option: ");
                WriteLine("1 Register");
                WriteLine("2 Login");
                Write("Your choice: ");
                try
                {
                    option = Convert.ToInt32(ReadLine());
                }
                catch
                {
                    option = -1;
                }

                if (option < 1 || option > 3) WriteLine("Invalid option, please try again.\n");
            }

            WriteLine();

            switch (option)
            {
                case 1:
                    string result = Register();
                    if (string.IsNullOrEmpty(result)) WriteLine("Registration was successful!");
                    else WriteLine($"Error:{result}");
                    Environment.Exit(0);
                    break;

                case 2:
                    if (Login() is null)
                    {
                        WriteLine("Invalid email or password!");
                        Environment.Exit(0);
                    }

                    break;

                default:
                    Help();
                    Environment.Exit(0);
                    break;
            }

            WriteLine("Login successful!");
        }
    }
}