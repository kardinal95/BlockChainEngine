using System.Configuration;

namespace BlockChainNode.Lib.Net
{
    static class Common
    {
        public static string HostName => ConfigurationManager.AppSettings["host"];
    }
}