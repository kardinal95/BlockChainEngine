using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockChainMachine.Core
{
    public class BlockChain
    {
        public List<Block> Chain { get; private set; }
        private List<Transaction> currentTransactions;
        public bool Pending => currentTransactions.Count != 0;
        public Block LastBlock => Chain.Last();

        public BlockChain()
        {
            Chain = new List<Block>();
            currentTransactions = new List<Transaction>();
            CreateNewBlock("genesis");
        }

        public void AddNewTransaction(Transaction trans)
        {
            currentTransactions.Add(trans);

            if (currentTransactions.Count < 10)
            {
                return;
            }

            CreateNewBlock();
        }

        private void CreateNewBlock(string previousHash = null)
        {
            if (previousHash is null)
            {
                previousHash = LastBlock.Hash;
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

        public bool TryAddBlock(Block block)
        {
            if (!block.Valid || block.PreviousHash != LastBlock.Hash ||
                block.Index != LastBlock.Index + 1)
            {
                return false;
            }

            Chain.Add(block);
            return true;
        }

        public void TryRebalance(List<Block> chain)
        {
            if (Chain.Count > chain.Count())
            {
                return;
            }

            for (var i = 1; i < chain.Count(); i++)
            {
                if (!chain[i].Valid || chain[i].PreviousHash != chain[i - 1].Hash ||
                    chain[i].Index != i + 1)
                {
                    return;
                }
            }

            Chain = chain;
        }

        private static string Timestamp(DateTime value)
        {
            return value.ToString("yyyy\'-\'MM\'-\'dd HH\':\'mm\':\'ss\'Z\'");
        }
    }
}