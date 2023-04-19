using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.Models
{
    [DataContract]
    public class EntryState
    {
        [DataMember]
        public DateTime DateTime { get; set; }

        [DataMember]
        public List<FieldGroup> Fields { get; set; } = new();
    }
}
