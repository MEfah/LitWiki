using LitWiki.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitWiki.Tools;
using System.Collections.ObjectModel;

namespace LitWiki.ViewModels
{
    public class SortingStrategyViewModel : ViewModelBase
    {
        // Not used


        SortingStrategy SortingStrategy { get; set; }


        public SortingType SortingType
        {
            get { return SortingStrategy.SortingType; }
            set
            {
                if(SortingStrategy.SortingType!= value)
                {
                    SortingStrategy.SortingType = value;
                    SortingTypeChanged?.Invoke(this, value);
                    OnPropertyChanged(nameof(SortingType));
                }
            }
        }


        private TemplateFieldViewModel? _templateFieldViewModel;
        public TemplateFieldViewModel? TemplateFieldViewModel
        {
            get { return _templateFieldViewModel; }
            set
            {
                if(_templateFieldViewModel != value)
                {
                    _templateFieldViewModel = value;
                    TemplateFieldViewModelChanged?.Invoke(this, value);
                    OnPropertyChanged(nameof(TemplateFieldViewModel));
                }
            }
        }


        public event EventHandler<SortingType> SortingTypeChanged;
        public event EventHandler<TemplateFieldViewModel?> TemplateFieldViewModelChanged;


        public SortingStrategyViewModel(SortingStrategy sortingStrategy)
        {
            SortingStrategy = sortingStrategy;
        }

        public SortingStrategyViewModel()
        {
            SortingStrategy = new SortingStrategy();
        }

        public int FindElementIndex(IGroupViewModel groupViewModel, IHierarchyElementViewModel elementViewModel)
        {
            if (SortingType == SortingType.ByField && TemplateFieldViewModel != null)
                return Array.BinarySearch(groupViewModel.ChildViewModels.Select(el => (EntryViewModel)el).ToArray(), (EntryViewModel)elementViewModel,
                    new DelegateComparer<EntryViewModel>((el1, el2) =>
                    {
                        int comp = el1!.Description.Fields[TemplateFieldViewModel.Position].CompareTo(el2!.Description.Fields[TemplateFieldViewModel.Position]);
                        return comp != 0 ? comp : el1!.Name.CompareTo(el2!.Name);
                    }));
            else
                return Array.BinarySearch(groupViewModel.ChildViewModels.ToArray(), elementViewModel,
                    new DelegateComparer<IHierarchyElementViewModel>((el1, el2) =>
                    {
                        return el1!.Name.CompareTo(el2!.Name);
                    }));
        }

        public ObservableCollection<IHierarchyElementViewModel> Sort(IGroupViewModel groupViewModel)
        {
            if(groupViewModel is DirectoryViewModel)
            {
                List<IHierarchyElementViewModel> elements = new();
                List<DirectoryViewModel> dirs = new();

                foreach (var item in groupViewModel.ChildViewModels)
                    if (item is DirectoryViewModel dir)
                        dirs.Add(dir);
                    else
                        elements.Add(item);

                var sortedDirs = dirs.OrderBy(el => el.Name);
                IOrderedEnumerable<IHierarchyElementViewModel> sortedElements;

                if (SortingType == SortingType.ByField && TemplateFieldViewModel != null)
                    sortedElements = elements.OrderBy(el => {
                        return ((EntryViewModel)el).Description.Fields[TemplateFieldViewModel.Position];
                    });
                else
                    sortedElements = elements.OrderBy(el => el.Name);

                return new(sortedDirs.Concat(sortedElements));
            }
            else
            {
                if (SortingType == SortingType.ByField && TemplateFieldViewModel != null)
                    return new(groupViewModel.ChildViewModels.OrderBy(el => {
                        return ((EntryViewModel)el).Description.Fields[TemplateFieldViewModel.Position];
                    }));
                else
                    return new(groupViewModel.ChildViewModels.OrderBy(el => el.Name));
            }
        }
    }
}
