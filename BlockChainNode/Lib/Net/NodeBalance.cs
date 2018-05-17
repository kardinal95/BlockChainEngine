using System;
using System.Collections.Generic;
using System.Net;
using BlockChainMachine.Core;
using BlockChainNode.Lib.Modules;
using Newtonsoft.Json;

namespace BlockChainNode.Lib.Net
{
    public static class NodeBalance
    {
        public static HashSet<string> NodeSet;

        public static string RegisterSelfAt(string node)
        {
            var parameters = new Dictionary<string, string> {{"host", Common.HostName}};
            var response = WebRequest.NewJsonPost($"{node}/comm/register", parameters);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException("Cannot connect with node network!");
            }

            var responseBody = WebRequest.GetJsonResponseBody(response);
            var nodeResponse = JsonConvert.DeserializeObject<NodeResponse>(responseBody);

            return nodeResponse.DataRows["Nodes"];
        }

        public static void BroadcastNewBlock()
        {
            foreach (var node in NodeSet)
            {
                if (node == Common.HostName)
                {
                    continue;
                }

                if (!NodeOnline(node))
                {
                    NodeSet.Remove(node);
                }
                else
                {
                    RegisterSelfAt(node);
                    SendNewBlock(node);
                }
            }
        }

        private static void SendNewBlock(string node)
        {
            var parameters = new Dictionary<string, string>
            {
                {"Block", JsonConvert.SerializeObject(OperationModule.Machine.LastBlock)}
            };
            try
            {
                var response = WebRequest.NewJsonPost($"{node}/bc/newblock", parameters);
            }
            catch (WebException)
            { }
        }

        private static void RebalanceSelfWith(string node)
        {
            var block = GetResponseItem<Block>(node, "/bc/lastblock", "Block");
            if (OperationModule.Machine.LastBlock.EqualTo(block))
            {
                return;
            }

            var chain = GetResponseItem<List<Block>>(node, "/bc/chain", "Chain");
            OperationModule.Machine.TryRebalance(chain);
        }

        private static bool NodeOnline(string node)
        {
            try
            {
                var response = WebRequest.NewJsonGet($"{node}/comm/info");
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (WebException)
            {
                return false;
            }
        }

        public static void RebalanceSelf()
        {
            foreach (var node in NodeSet)
            {
                if (node == Common.HostName)
                {
                    continue;
                }

                if (!NodeOnline(node))
                {
                    NodeSet.Remove(node);
                }
                else
                {
                    RegisterSelfAt(node);
                    RebalanceSelfWith(node);
                }
            }
        }

        private static T GetResponseItem<T>(string node, string uri, string row)
        {
            var response = WebRequest.NewJsonGet($"{node}{uri}");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException("Node did not return correct information");
            }

            var json = WebRequest.GetJsonResponseBody(response);
            var nodeResponse = JsonConvert.DeserializeObject<NodeResponse>(json);
            var item = JsonConvert.DeserializeObject<T>(nodeResponse.DataRows[row]);
            return item;
        }
    }
}