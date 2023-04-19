using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.Models
{
    public interface IHierarchyElement : IGrouppable
    {
        string Name { get; set; }
    }
}
