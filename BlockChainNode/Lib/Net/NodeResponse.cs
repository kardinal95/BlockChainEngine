using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nancy;

namespace BlockChainNode.Lib.Net
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    struct NodeResponse
    {
        public string Host { get; set; }
        public string ResponseString { get; set; }
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