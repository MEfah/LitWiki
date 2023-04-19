using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.Models
{
    [DataContract(IsReference = true)]
    public class Entry : IHierarchyElement
    {
        [DataMember]
        public string Name { get; set; } = string.Empty;
        public IGroup ParentGroup { get; set; }

        [DataMember]
        public List<EntryState> States { get; set; } = new();

        [DataMember]
        public List<FieldGroup> FieldGroups { get; set; } = new();
    }
}
