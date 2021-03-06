﻿using System;
using System.Collections.Generic;
using System.Net;
using BlockChainMachine.Core;
using BlockChainNode.Modules;
using Newtonsoft.Json;

namespace BlockChainNode.Net
{
    public static class NodeBalance
    {
        public static HashSet<string> NodeSet;

        public static void PerformOnAllNodes(Action<string> act)
        {
            foreach (var node in NodeSet)
            {
                if (!NodeOnline(node))
                {
                    NodeSet.Remove(node);
                }
                else
                {
                    act.Invoke(node);
                }
            }
        }

        public static string RegisterThisNode(string node, string target)
        {
            var parameters = new Dictionary<string, string> {{"host", target}};
            var response = Common.NewJsonPost($"{node}/comm/register", parameters);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException("Cannot connect with node network!");
            }

            return Common.GetJsonResponseBody(response);
        }

        public static void RebalanceNode(string node)
        {
            var chain = GetNodeChain(node);
            OperationModule.Machine.RebalanceWith(chain);
        }

        public static bool NodeOnline(string node)
        {
            var response = Common.NewJsonGet($"{node}/comm/info");
            return response.StatusCode == HttpStatusCode.OK;
        }

        public static void SyncNode(string node)
        {
            var parameters = new Dictionary<string, string>();
            Common.NewJsonPost($"{node}/comm/sync", parameters);
        }

        public static List<Block> GetNodeChain(string node)
        {
            var response = Common.NewJsonGet($"{node}/bc/chain");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException("Node did not return correct information");
            }

            var json = Common.GetJsonResponseBody(response);
            return JsonConvert.DeserializeObject<List<Block>>(
                json, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
        }
    }
}