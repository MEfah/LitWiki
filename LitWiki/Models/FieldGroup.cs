using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.Models
{
    [DataContract]
    public class FieldGroup
    {
        [DataMember]
        public Template BasedOn { get; set; } = new();

        [DataMember]
        public List<Field> Fields { get; set; } = new(); 
    }
}
