using System.Collections.Generic;

namespace BlockChainMachine.Core
{
    /// <summary>
    ///     Один блок целой цепи
    /// </summary>
    public struct Block
    {
        public int Index;
        public List<ITransaction> Transactions;
        public string PreviousHash;
        public string TimeStamp;
        public bool IsValid => ValidBlock();

        private bool ValidBlock()
        {
            foreach (var transaction in Transactions)
            {
                // TODO Signature checks
                if (transaction.Signature == null)
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return $"Block: No. {Index}, with {Transactions.Count} transactions.\n" +
                   $"Previous hash: {PreviousHash}, created on {TimeStamp}.";
        }
    }
}