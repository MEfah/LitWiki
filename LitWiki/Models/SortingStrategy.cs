using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.Models
{
    public enum SortingType
    {
        ByName,
        ByField
    }

    public class SortingStrategy
    {
        // Not used

        public SortingType SortingType { get; set; }
        public string FieldName { get; set; } = string.Empty;
    }
}
