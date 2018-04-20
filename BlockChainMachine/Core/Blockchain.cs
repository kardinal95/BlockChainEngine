using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BlockChainMachine.Core
{
    public class BlockChain
    {
        public List<Block> Chain { get; private set; }
        private List<ITransaction> currentTransactions;
        public Block LastBlock => Chain.Last();
        public bool Pending => currentTransactions.Count != 0;
        public bool HaveValidChain => ValidChain(Chain);

        public BlockChain()
        {
            Chain = new List<Block>();
            currentTransactions = new List<ITransaction>();
            AddNewBlock("genesis");
        }

        public bool ValidChain(List<Block> chain)
        {
            // Проверяем наличие блоков в целом и генезис-блока
            if (chain.Count < 1 || chain[0].PreviousHash != "genesis")
            {
                return false;
            }

            for (var current = 1; current < chain.Count; current++)
            {
                if (chain[current].Index != current + 1 ||
                    chain[current].PreviousHash != Hash(chain[current - 1]) ||
                    !chain[current].IsValid)
                {
                    return false;
                }
            }

            return true;
        }

        public void RebalanceWith(List<Block> chain)
        {
            if (Chain.Count >= chain.Count)
            {
                return;
            }

            if (ValidChain(chain))
            {
                Chain = chain;
            }
        }

        public int AddNewTransaction(ITransaction trans)
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

            currentTransactions = new List<ITransaction>();
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