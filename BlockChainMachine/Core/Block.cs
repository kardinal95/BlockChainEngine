using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace BlockChainMachine.Core
{
    [Serializable]
    public struct Block : ISerializable
    {
        public int Index;
        public string PreviousHash;
        public string TimeStamp;
        public List<Transaction> Transactions;
        public bool Valid => Validate();
        public string Hash => GetBlockHash();

        private bool Validate()
        {
            foreach (var transaction in Transactions)
            {
                if (!transaction.Valid)
                {
                    return false;
                }
            }

            return true;
        }

        private string GetBlockHash()
        {
            var hash = new StringBuilder();
            var hashFunc = new SHA256Managed();
            var crypt = hashFunc.ComputeHash(GetBytes());
            foreach (var item in crypt)
            {
                hash.Append(item.ToString("x2"));
            }

            return hash.ToString();
        }

        private byte[] GetBytes()
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);
                return stream.ToArray();
            }
        }

        public override string ToString()
        {
            return $"Block: No. {Index}, with {Transactions.Count} transactions.\n" +
                   $"Previous hash: {PreviousHash}, created on {TimeStamp}.";
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Index", Index);
            info.AddValue("Transactions", Transactions);
            info.AddValue("PreviousHash", PreviousHash);
            info.AddValue("TimeStamp", TimeStamp);
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public Block(SerializationInfo info, StreamingContext context)
        {
            Index = (int) info.GetValue("Index", typeof(int));
            Transactions = (List<Transaction>) info.GetValue("Transactions", typeof(List<Transaction>));
            PreviousHash = (string) info.GetValue("PreviousHash", typeof(string));
            TimeStamp = (string) info.GetValue("TimeStamp", typeof(string));
        }

        public bool EqualTo(Block target)
        {
            return Hash == target.Hash;
        }
    }
}