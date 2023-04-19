using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.ViewModels
{
    public class SelectEntryWindowViewModel : ViewModelBase, IModalDialogViewModel, IDialogDependentViewModel
    {
        private string _searchString;
        public string SearchString
        {
            get { return _searchString; }
            set
            {
                if(_searchString != value)
                {
                    _searchString = value;
                    OnPropertyChanged(nameof(SearchString));
                }
            }
        }


        private Dictionary<string, EntryViewModel> _availableEntries;


        public bool? DialogResult { get; private set; } = false;


        public bool Multiple { get; private set; }


        public RelayCommand AcceptCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand SearchCommand { get; }


        public ObservableCollection<SelectionEntryViewModel> DisplayedViewModels { get; private set; }
        public IDialogViewModel DialogViewModel { get; }
        public SelectEntryWindowViewModel(ICollection<EntryViewModel> entryViewModels, ICollection<EntryViewModel> selected,
            IDialogViewModel dialogViewModel, bool multiple = false)
        {
            Multiple = multiple;
            DialogViewModel = dialogViewModel;

            DisplayedViewModels = new();
            var sorted = entryViewModels.OrderBy(el => el.Name);
            foreach(var item in sorted)
            {
                bool isSelected = false;
                if(selected.Remove(item))
                    isSelected = true;
                DisplayedViewModels.Add(new SelectionEntryViewModel(item, isSelected));
            }

            AcceptCommand = new(Accept);
            CancelCommand = new(Cancel);
            SearchCommand = new(Search);
        }


        public IList<EntryViewModel> GetSelected()
        {
            return DisplayedViewModels.Where(el => el.IsSelected).Select(el => el.EntryViewModel).ToList();
        }

        private void Accept()
        {
            DialogResult = true;
            DialogViewModel.DialogService.Close(this);
        }

        private void Cancel()
        {
            DialogResult = false;
            DialogViewModel.DialogService.Close(this);
        }

        private void Search()
        {
            if(SearchString != null && SearchString != "")
                foreach(var item in DisplayedViewModels)
                    item.IsVisible = item.EntryViewModel.Name.ToLower().Contains(SearchString.ToLower());
            else
                foreach(var item in DisplayedViewModels)
                    item.IsVisible = true;
        }
    }

    public class SelectionEntryViewModel : ViewModelBase
    {
        public EntryViewModel EntryViewModel { get; private set; }


        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if(_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }


        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if(_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }


        public SelectionEntryViewModel(EntryViewModel entryViewModel, bool isSelected)
        {
            EntryViewModel = entryViewModel;
            IsSelected = isSelected;
        }
    }
}
