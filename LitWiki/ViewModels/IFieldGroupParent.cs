using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.ViewModels
{
    interface IFieldGroupParent
    {
        public ReadOnlyObservableCollection<FieldGroupViewModel> FieldGroupsViewModels { get; }

        public bool HideEmptyFields { get; set; }
    }
}
