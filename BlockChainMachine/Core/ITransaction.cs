namespace BlockChainMachine.Core
{
    public interface ITransaction
    {
        string Data { get; set; }
        bool HasValidData { get; }
        string UserHash { get; set; }
        byte[] Signature { get; set; }
        // TODO Add Public Key
    }
}