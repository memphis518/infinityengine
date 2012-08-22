using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace InfinityEngine.Models
{
    [DataContract]
    public class Configuration
    {

        [DataMember]
        public string UpdateURL {get; set;}

        [DataMember]
        public string RecordIndentifier { get; set; } 

        [DataMember]
        public string AutoCompleteRoute { get; set; } 

        [DataMember]
        public int MaxResults {get; set;} 

    }
}
