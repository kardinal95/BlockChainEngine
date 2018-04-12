using System.Collections.Generic;

namespace BlockChainEngine.Core
{
    public struct Block
    {
        public int Index { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string PreviousHash { get; set; }
        public string TimeStamp { get; set; }
    }
}