using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace InfinityEngine.Models
{
    [DataContract]
    public class Configuration
    {
        
        [DataMember]
        public string UpdateURL {get; set;} //Url used to update the route's cache

        [DataMember]
        public string RecordIndentifier { get; set; } //Unique id for each data record

        [DataMember]
        public string AutoCompleteRoute { get; set; } //Name of the route that is used for searching and updating

        [DataMember]
        public int MaxResults {get; set;} //MaxResults for a given route

    }
}
