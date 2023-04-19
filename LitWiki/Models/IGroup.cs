using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.Models
{
    public interface IGroup
    {
        public SortingStrategy SortingStrategy { get; set; }
        public List<IGrouppable> Elements { get; set; }
    }


    public interface IGrouppable
    {
        public IGroup ParentGroup { get; set; }
    }
}
