using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.Models
{
    [KnownType(typeof(List<Entry>))]
    [DataContract(IsReference = true)]
    public class Field
    {
        [DataMember]
        public TemplateField TemplateField { get; set; } = new();

        [DataMember]
        public object? Value { get; set; } = default;
    }
}
