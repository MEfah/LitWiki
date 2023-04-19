using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LitWiki.Models
{
    [DataContract]
    [KnownType(typeof(Directory))]
    [KnownType(typeof(Group))]
    [KnownType(typeof(Entry))]
    public class Group : IHierarchyElement, IGroup
    {
        [DataMember]
        public string Name { get; set; } = string.Empty;
        public SortingStrategy SortingStrategy { get; set; }
        public IGroup ParentGroup { get; set; }

        [DataMember]
        public List<IGrouppable> Elements { get; set; } = new();


        public Group()
        {
            Elements = new();
        }
    }
}
