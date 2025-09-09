using System;
using System.Collections.Generic;
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
    }

    public class BankAccount : IAccount
    {
        public virtual void Deposit(decimal amount) { }
        public virtual bool Withdraw(decimal amount) { return false; }
        public decimal GetBalance() { return 0; }
    }

    public class SavingsAccount : BankAccount
    {
        public void ApplyInterest() { }
        public override bool Withdraw(decimal amount) { return false; }
    }

    public sealed class FixedDepositAccount : BankAccount
    {
        public override bool Withdraw(decimal amount) { return false; }
    }

}
