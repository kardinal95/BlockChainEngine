using System.Reflection;
using log4net;

namespace BlockChainNode.Lib.Logging
{
    static class Logger
    {
        public static ILog Log { get; private set; }

        public static void Init()
        {
            Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}