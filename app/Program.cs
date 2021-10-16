using System;
using static System.Console;
using System.Collections.Generic;

namespace qm95
{
    class Program
    {
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

        private static int CreateAccount()
        {
            int account;
            do
            {
                WriteLine("\nChoose an account type: ");
                foreach (AccountType type in Enum.GetValues(typeof(AccountType)))
                {
                    WriteLine($"{Convert.ToInt32(type)} {type}");
                }

                Write("Your choice: ");
                try
                {
                    account = Convert.ToInt32(ReadLine());
                }
                catch
                {
                    account = -1;
                }

                if (account < 1 || account > Enum.GetValues(typeof(AccountType)).Length)
                    WriteLine("Invalid option, please try again.\n");
            } while (account < 1 || account > Enum.GetValues(typeof(AccountType)).Length);

            return account;
        }

        private static int GetAccountIndex(IReadOnlyList<Account> accounts)
        {
            int account;
            do
            {
                WriteLine("\nChoose an account: ");
                for (int i = 0; i < accounts.Count; i++)
                {
                    WriteLine($"{i} {accounts[i].Type}");
                }

                Write("Your choice: ");
                try
                {
                    account = Convert.ToInt32(ReadLine());
                }
                catch
                {
                    account = -1;
                }

                if (account < 0 || account >= accounts.Count)
                    WriteLine("Invalid option, please try again.\n");
            } while (account < 0 || account >= accounts.Count);

            return account;
        }

        private static int GetTransferType()
        {
            int transferType;
            do
            {
                WriteLine("\nChoose transfer type: ");
                foreach (TransferType type in Enum.GetValues(typeof(TransferType)))
                {
                    WriteLine($"{Convert.ToInt32(type)} {type}");
                }

                Write("Your choice: ");
                try
                {
                    transferType = Convert.ToInt32(ReadLine());
                }
                catch
                {
                    transferType = -1;
                }

                if (transferType < 1 || transferType > Enum.GetValues(typeof(TransferType)).Length)
                    WriteLine("Invalid option, please try again.\n");
            } while (transferType < 1 || transferType > Enum.GetValues(typeof(TransferType)).Length);

            return transferType;
        }

        private static decimal GetAmount(TransferType type, Account account)
        {
            decimal amount;
            do
            {
                Write("\nEnter transfer amount: ");
                try
                {
                    amount = Convert.ToDecimal(ReadLine());
                }
                catch
                {
                    amount = -1;
                }

                if (amount < 1) WriteLine("Minimum amount is 1, please try again.\n");
                if (type == TransferType.Withdraw && account.Balance - amount < (decimal) 0.0)
                    WriteLine("Not enough money, please try again.\n");
            } while (amount < 1 || type == TransferType.Withdraw && account.Balance - amount < (decimal) 0.0);

            return amount;
        }

        private static void Main()
        {
            Title = "qm95";

            string[] mainOptions = {"1 Register", "2 Login"};

            int option = -1;
            while (option < 1 || option > mainOptions.Length)
            {
                WriteLine("Choose an option: ");
                foreach (var s in mainOptions) WriteLine(s);

                Write("Your choice: ");
                try
                {
                    option = Convert.ToInt32(ReadLine());
                }
                catch
                {
                    option = -1;
                }

                if (option < 1 || option > mainOptions.Length) WriteLine("Invalid option, please try again.\n");
            }

            WriteLine();

            Customer customer = new Customer();

            string result;
            switch (option)
            {
                case 1:
                    result = Register();
                    WriteLine(string.IsNullOrEmpty(result) ? "Registration was successful!" : $"Error:{result}");
                    Environment.Exit(0);
                    break;

                case 2:
                    if ((customer = Login()) is null)
                    {
                        WriteLine("Invalid email or password!");
                        Environment.Exit(0);
                    }

                    break;
            }

            WriteLine("Login successful!\n\n");

            string[] opt =
            {
                "1 Create an account", "2 Close an account", "3 View all accounts", "4 Create a transfer",
                "5 View account transfers", "6 Exit"
            };

            int index;
            while (true)
            {
                do
                {
                    WriteLine("\nChoose an option: ");
                    foreach (var s in opt) WriteLine(s);

                    Write("Your choice: ");
                    try
                    {
                        option = Convert.ToInt32(ReadLine());
                    }
                    catch
                    {
                        option = -1;
                    }

                    if (option < 1 || option > opt.Length) WriteLine("Invalid option, please try again.\n");
                } while (option < 1 || option > opt.Length);

                switch (option)
                {
                    case 1:
                        AccountType type = (AccountType) CreateAccount();
                        result = customer.CreateAccount(type);
                        WriteLine(result is null ? "Account was created!" : $"Error: {result}");
                        break;

                    case 2:
                        if (customer.Accounts.Count == 0)
                        {
                            WriteLine("No accounts!");
                            break;
                        }

                        result = customer.CloseAccount(GetAccountIndex(customer.Accounts));
                        WriteLine(result is null ? "Account was deleted!" : $"Error: {result}");
                        break;

                    case 3:
                        if (customer.Accounts.Count == 0)
                        {
                            WriteLine("No accounts!");
                            break;
                        }

                        WriteLine("Type || Balance || Opening date");
                        foreach (var account in customer.Accounts)
                        {
                            WriteLine("{0,-5} || {1:C} || {2:yyyy-MM-dd}", account.Type, account.Balance,
                                account.OpeningDate);
                        }
                        break;

                    case 4:
                        index = GetAccountIndex(customer.Accounts);
                        TransferType transferType = (TransferType) GetTransferType();
                        if (transferType == TransferType.Withdraw && customer.Accounts[index].Balance < 1)
                        {
                            WriteLine("Not enough money!");
                            break;
                        }

                        decimal amount = GetAmount(transferType, customer.Accounts[index]);
                        result = customer.Accounts[index].MakeTransfer(amount, transferType);
                        WriteLine(result is null ? "Transfer was completed!" : $"Error: {result}");
                        break;

                    case 5:
                        if (customer.Accounts.Count == 0)
                        {
                            WriteLine("No accounts!");
                            break;
                        }

                        index = GetAccountIndex(customer.Accounts);

                        WriteLine("Amount || Date || Type");
                        foreach (var transfer in customer.Accounts[index].Transfers)
                        {
                            WriteLine("{0:C} || {1:yyyy-MM-dd HH:mm} || {2}", transfer.Amount, transfer.TransferDate,
                                transfer.Type);
                        }

                        break;
                    case 6:
                        return;
                }
            }
        }
    }
}