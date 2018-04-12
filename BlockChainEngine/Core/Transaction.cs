namespace BlockChainEngine.Core
{
    public struct Transaction
    {
        public string PollId { get; set; }
        public string OptionId { get; set; }
        public string UserHash { get; set; }

        public override string ToString()
        {
            return $"Transaction: Poll {PollId} - Opt. {OptionId} by {UserHash}";
        }
    }
}