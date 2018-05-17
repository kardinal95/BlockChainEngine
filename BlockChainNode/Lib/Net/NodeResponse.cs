using System.Collections.Generic;
using System.Linq;
using Nancy;

namespace BlockChainNode.Lib.Net
{
    struct NodeResponse
    {
        public string Host { private get; set; }
        public string ResponseString { private get; set; }
        public HttpStatusCode HttpCode { get; set; }
        public Dictionary<string, string> DataRows { get; set; }

        public override string ToString()
        {
            var dataStrings = DataRows.Select(x => $"{x.Key}: {x.Value}");

            return $"NodeResponse, HTTP Code: {HttpCode}\n" +
                   $"Host: {Host}, ResponseString: {ResponseString}\n" + "DataRows: " +
                   string.Join("\n", dataStrings);
        }
    }
}