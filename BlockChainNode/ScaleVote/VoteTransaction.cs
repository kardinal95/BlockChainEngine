using BlockChainMachine.Core;

namespace BlockChainNode.ScaleVote
{
    class VoteTransaction : ITransaction
    {
        public string Data { get; set; }

        public bool HasValidData => Data != string.Empty;

        public string UserHash { get; set; }
        public byte[] Signature { get; set; }
    }
}