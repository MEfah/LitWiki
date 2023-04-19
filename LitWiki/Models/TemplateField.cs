using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.Models
{
    public enum FieldType
    {
        String,
        Text,
        Number,
        DateTime,
        Image,
        Entry,
        ListOfEntries
    }

    [DataContract(IsReference = true)]
    public class TemplateField
    {
        [DataMember]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        public FieldType FieldType { get; set; }

        [DataMember]
        public Directory? EntrySource { get; set; }

        [DataMember]
        public int Position { get; set; }
    }
}
