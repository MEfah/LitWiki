using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LitWiki.Models
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(Directory))]
    [KnownType(typeof(Group))]
    [KnownType(typeof(Entry))]
    public class Directory : IHierarchyElement, IGroup
    {
        [DataMember]
        public string Name { get; set; } = string.Empty;
        public SortingStrategy SortingStrategy { get; set; }
        public GroupingType GroupingType { get; set; }

        [DataMember]
        public Template EntriesTemplate { get; set; } = new();
        [DataMember]
        public Template StatesTemplate { get; set; } = new();

        [DataMember]
        public List<IGrouppable> Elements { get; set; } = new();

        public IGroup ParentGroup { get; set; }



        public Directory()
        {
            Elements = new();
        }
    }
}
