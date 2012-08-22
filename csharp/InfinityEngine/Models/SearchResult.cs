using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace InfinityEngine.Models
{
    [DataContract]
    public class SearchResult
    {
        [DataMember]
        string id { get; set; }

        [DataMember]
        string value { get; set; }

    }
}
