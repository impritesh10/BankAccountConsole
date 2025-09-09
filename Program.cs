using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountConsole
{
    public interface IAccount
    {
        void Deposit(decimal amount);
        bool Withdraw(decimal amount);
        decimal GetBalance();
        string AccountNumber { get; }
        string GetAccountType();
    }

    public class BankAccount : IAccount
    {
        protected decimal balance;
        public string AccountNumber { get; private set; }
        public string OwnerName { get; private set; }

        public BankAccount(string accountNumber, string ownerName, decimal initialBalance)
        {
            if (string.IsNullOrWhiteSpace(accountNumber)) throw new ArgumentException("accountNumber required");
            AccountNumber = accountNumber;
            OwnerName = ownerName ?? "Unknown";
            balance = initialBalance < 0 ? 0 : initialBalance;
        }

        public virtual void Deposit(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive");
            balance += amount;
        }

        public virtual bool Withdraw(decimal amount)
        {
            if (amount <= 0) return false;
            if (amount > balance) return false;
            balance -= amount;
            return true;
        }

        public decimal GetBalance()
        {
            return balance;
        }

        public virtual string GetAccountType()
        {
            return "BankAccount";
        }

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2} balance: ₹{3}",
                AccountNumber, OwnerName, GetAccountType(), balance.ToString("N2", CultureInfo.InvariantCulture));
        }
    }

    public class SavingsAccount : BankAccount
    {
        public decimal InterestRate { get; private set; } // e.g., 0.03m = 3%

        public SavingsAccount(string accountNumber, string ownerName, decimal initialBalance, decimal interestRate)
            : base(accountNumber, ownerName, initialBalance)
        {
            InterestRate = interestRate < 0 ? 0 : interestRate;
        }

        public void ApplyInterest()
        {
            if (InterestRate <= 0) return;
            var interest = balance * InterestRate;
            // use Deposit to reuse validation
            Deposit(Decimal.Round(interest, 2));
        }

        public override bool Withdraw(decimal amount)
        {
            // Example rule: keep minimum balance 100
            const decimal minBalance = 100m;
            if (amount <= 0) return false;
            if (balance - amount < minBalance) return false;
            return base.Withdraw(amount);
        }

        public override string GetAccountType()
        {
            return "SavingsAccount";
        }
    }

    public sealed class FixedDepositAccount : BankAccount
    {
        public DateTime MaturityDate { get; private set; }

        public FixedDepositAccount(string accountNumber, string ownerName, decimal initialBalance, DateTime maturity)
            : base(accountNumber, ownerName, initialBalance)
        {
            MaturityDate = maturity;
        }

        public override bool Withdraw(decimal amount)
        {
            if (DateTime.Now < MaturityDate) return false;
            return base.Withdraw(amount);
        }

        public override string GetAccountType()
        {
            return "FixedDeposit";
        }
    }

    public class AccountManager
    {
        private readonly List<IAccount> accounts = new List<IAccount>();

        public void Add(IAccount acc) { accounts.Add(acc); }

        public IAccount Find(string accountNumber)
        {
            foreach (var a in accounts)
            {
                if (string.Equals(a.AccountNumber, accountNumber, StringComparison.OrdinalIgnoreCase))
                    return a;
            }
            return null;
        }

        public IEnumerable<IAccount> ListAll() { return accounts; }
    }

    class Program
    {
        static AccountManager mgr = new AccountManager();

        static void Main()
        {
            SeedDemoData();

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== BankAccount Console ===");
                Console.WriteLine("1. Create account");
                Console.WriteLine("2. Deposit");
                Console.WriteLine("3. Withdraw");
                Console.WriteLine("4. Show balance");
                Console.WriteLine("5. List accounts");
                Console.WriteLine("6. Apply interest to savings");
                Console.WriteLine("7. Exit");
                Console.Write("Choose: ");
                var choice = Console.ReadLine();

                if (choice == "7") break;

                try
                {
                    switch (choice)
                    {
                        case "1": CreateAccount(); break;
                        case "2": Deposit(); break;
                        case "3": Withdraw(); break;
                        case "4": ShowBalance(); break;
                        case "5": ListAccounts(); break;
                        case "6": ApplyInterest(); break;
                        default: Console.WriteLine("Invalid choice"); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        static void SeedDemoData()
        {
            mgr.Add(new BankAccount("B100", "Alice", 500m));
            mgr.Add(new SavingsAccount("S200", "Bob", 1500m, 0.03m));
            mgr.Add(new FixedDepositAccount("F300", "Charlie", 5000m, DateTime.Now.AddDays(30)));
        }

        static void CreateAccount()
        {
            Console.Write("Account type (bank/savings/fixed): ");
            var type = Console.ReadLine().Trim().ToLower();

            Console.Write("Account number: ");
            var accNo = Console.ReadLine().Trim();

            Console.Write("Owner name: ");
            var owner = Console.ReadLine().Trim();

            Console.Write("Initial balance: ");
            decimal initial;
            if (!decimal.TryParse(Console.ReadLine(), out initial)) initial = 0m;

            if (type == "savings")
            {
                Console.Write("Interest rate (e.g., 0.03 for 3%): ");
                decimal rate;
                if (!decimal.TryParse(Console.ReadLine(), out rate)) rate = 0m;
                mgr.Add(new SavingsAccount(accNo, owner, initial, rate));
                Console.WriteLine("Savings account created.");
            }
            else if (type == "fixed")
            {
                Console.Write("Maturity date (yyyy-MM-dd): ");
                DateTime mat;
                if (!DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out mat))
                {
                    mat = DateTime.Now.AddMonths(1);
                    Console.WriteLine("Invalid date, defaulting to " + mat.ToString("yyyy-MM-dd"));
                }
                mgr.Add(new FixedDepositAccount(accNo, owner, initial, mat));
                Console.WriteLine("Fixed deposit account created.");
            }
            else
            {
                mgr.Add(new BankAccount(accNo, owner, initial));
                Console.WriteLine("Bank account created.");
            }
        }

        static void Deposit()
        {
            Console.Write("Account number: ");
            var accNo = Console.ReadLine().Trim();
            var acc = mgr.Find(accNo);
            if (acc == null) { Console.WriteLine("Account not found"); return; }

            Console.Write("Amount: ");
            decimal amt;
            if (!decimal.TryParse(Console.ReadLine(), out amt)) { Console.WriteLine("Invalid amount"); return; }

            acc.Deposit(amt);
            Console.WriteLine("Deposit successful. New balance: ₹" + acc.GetBalance().ToString("N2", CultureInfo.InvariantCulture));
        }

        static void Withdraw()
        {
            Console.Write("Account number: ");
            var accNo = Console.ReadLine().Trim();
            var acc = mgr.Find(accNo);
            if (acc == null) { Console.WriteLine("Account not found"); return; }

            Console.Write("Amount: ");
            decimal amt;
            if (!decimal.TryParse(Console.ReadLine(), out amt)) { Console.WriteLine("Invalid amount"); return; }

            var ok = acc.Withdraw(amt);
            Console.WriteLine(ok ? "Withdraw successful. New balance: ₹" + acc.GetBalance().ToString("N2", CultureInfo.InvariantCulture) : "Withdraw failed (insufficient funds or rule)");
        }

        static void ShowBalance()
        {
            Console.Write("Account number: ");
            var accNo = Console.ReadLine().Trim();
            var acc = mgr.Find(accNo);
            if (acc == null) { Console.WriteLine("Account not found"); return; }
            Console.WriteLine(acc.ToString());
        }

        static void ListAccounts()
        {
            foreach (var a in mgr.ListAll())
            {
                Console.WriteLine(a.ToString());
            }
        }

        static void ApplyInterest()
        {
            foreach (var a in mgr.ListAll())
            {
                var s = a as SavingsAccount;
                if (s != null)
                {
                    s.ApplyInterest();
                    Console.WriteLine("Applied interest to " + s.AccountNumber);
                }
            }
            Console.WriteLine("Done.");
        }
        

    }


}


