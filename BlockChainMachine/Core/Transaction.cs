namespace BlockChainMachine.Core
{
    public struct Transaction
    {
        public string PollId;
        public string OptionId;
        public string UserHash;

        // TODO Add following: openkey, signature
        // TODO Add methods for: checking signature with key & hash

        public override string ToString()
        {
            return $"Transaction: Poll {PollId} - Opt. {OptionId} by {UserHash}";
        }
    }
}