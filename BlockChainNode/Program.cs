using System;
using System.Collections.Generic;
using System.Configuration;
using BlockChainNode.Net;
using Nancy.Hosting.Self;
using Newtonsoft.Json;

namespace BlockChainNode
{
    public class Program
    {
        public static void Main()
        {
            var hostUri = new Uri(ConfigurationManager.AppSettings["host"]);

            NodeBalance.NodeSet = new HashSet<string> {hostUri.ToString()};
            try
            {
                var hosts =
                    NodeBalance.RegisterThisNode(ConfigurationManager.AppSettings["target"],
                                                 hostUri.ToString());
                NodeBalance.NodeSet = JsonConvert.DeserializeObject<HashSet<string>>(hosts);
                NodeBalance.SyncNode(ConfigurationManager.AppSettings["host"]);
            }
            catch (ApplicationException exc)
            {
                Console.WriteLine(exc.Message);
                Console.ReadLine();
                return;
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("No nodes found. Ignore this if it is the only node in network");
            }

            var host = new NancyHost(hostUri);
            var running = true;

            host.Start();
            while (running is true)
            {
                var input = Console.ReadLine();
                if (input == "exit" || input == "quit")
                {
                    running = false;
                }
            }

            host.Stop();
        }
    }
}