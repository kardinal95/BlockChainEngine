using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BlockChainEngine.Core
{
    public class BlockChain
    {
        public List<Block> Chain { get; }
        private readonly List<Transaction> currentTransactions;
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
            currentTransactions.Add(trans);

            return LastBlock.Index + 1;
        }

        public int AddNewTransaction(Transaction trans)
        {
            var copy = trans;
            currentTransactions.Add(copy);

            if (currentTransactions.Count < 10)
            {
                return LastBlock.Index + 1;
            }

            AddNewBlock();
            return LastBlock.Index;
        }

        public void AddNewBlock(string previousHash = null)
        {
            var timestamp = DateTime.Now.ToBinary().ToString();
            if (previousHash is null)
            {
                previousHash = Hash(LastBlock);
            }

            var block = new Block
            {
                Index = Chain.Count + 1,
                Transactions = currentTransactions,
                PreviousHash = previousHash,
                TimeStamp = timestamp
            };

            currentTransactions.Clear();
            Chain.Add(block);
        }

        public static string Hash(Block block)
        {
            var blockString = block.ToString();
            var crypt = new SHA256Managed();
            var hashed = crypt.ComputeHash(Encoding.UTF8.GetBytes(blockString));
            return hashed.ToString();
        }
    }
}