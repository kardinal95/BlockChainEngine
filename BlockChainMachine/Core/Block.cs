using System.Collections.Generic;

namespace BlockChainMachine.Core
{
    /// <summary>
    ///     Один блок целой цепи
    /// </summary>
    public struct Block
    {
        public int Index;
        public List<Transaction> Transactions;
        public string PreviousHash;
        public string TimeStamp;

        public override string ToString()
        {
            return $"Block: No. {Index}, with {Transactions.Count} transactions.\n" +
                   $"Previous hash: {PreviousHash}, created on {TimeStamp}.";
        }
    }
}