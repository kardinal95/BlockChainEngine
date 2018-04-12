using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BlockChainMachine.Core
{
    public class BlockChain
    {
        public List<Block> Chain { get; }
        private List<Transaction> currentTransactions;
        public Block LastBlock => Chain.Last();
        public bool Pending => currentTransactions.Count != 0;

        public BlockChain()
        {
            Chain = new List<Block>();
            currentTransactions = new List<Transaction>();
            AddNewBlock("genesis");
        }

        public int AddNewTransaction(string pollId, string optionId, string userHash)
        {
            var trans = new Transaction {PollId = pollId, OptionId = optionId, UserHash = userHash};
            return AddNewTransaction(trans);
        }

        public int AddNewTransaction(Transaction trans)
        {
            currentTransactions.Add(trans);

            if (currentTransactions.Count < 10)
            {
                return LastBlock.Index + 1;
            }

            AddNewBlock();
            return LastBlock.Index;
        }

        public void AddNewBlock(string previousHash = null)
        {
            if (previousHash is null)
            {
                previousHash = Hash(LastBlock);
            }

            var block = new Block
            {
                Index = Chain.Count + 1,
                Transactions = currentTransactions,
                PreviousHash = previousHash,
                TimeStamp = Timestamp(DateTime.Now)
            };

            currentTransactions = new List<Transaction>();
            Chain.Add(block);
        }

        public static string Hash(Block block)
        {
            var hash = new StringBuilder();
            var hashFunc = new SHA256Managed();
            var crypt = hashFunc.ComputeHash(Encoding.UTF8.GetBytes(block.ToString()));
            foreach (var item in crypt)
            {
                hash.Append(item.ToString("x2"));
            }

            return hash.ToString();
        }

        public static string Timestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
    }
}