using CommunityToolkit.Mvvm.Input;
using LitWiki.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.ViewModels
{
    public class EntryStateViewModel : DialogDependentViewModelBase, IEditeableViewModel, IFieldGroupParent
    {
        public EntryState EntryState { get; set; }


        private bool _isEdited;
        public bool IsEdited
        {
            get { return _isEdited; }
            set
            {
                if (_isEdited != value)
                {
                    _isEdited = value;
                    OnPropertyChanged(nameof(IsEdited));
                }
            }
        }


        private bool _showEmptyFields;
        public bool HideEmptyFields
        {
            get { return _showEmptyFields; }
            set
            {
                if (_showEmptyFields != value)
                {
                    _showEmptyFields = value;
                    foreach (var fieldGroup in FieldGroupsViewModels)
                        fieldGroup.HideEmptyFields = _showEmptyFields;
                }
            }
        }


        public DateTime DateTime
        {
            get { return EntryState.DateTime; }
            set
            {
                if(EntryState.DateTime != value)
                {
                    EntryState.DateTime = value;
                    OnPropertyChanged(nameof(DateTime));
                }
            }
        }


        public FieldGroupViewModel Description { get; }
        private ObservableCollection<FieldGroupViewModel> _fieldGroupsViewModels;
        public ReadOnlyObservableCollection<FieldGroupViewModel> FieldGroupsViewModels { get; }


        public RelayCommand ChangeDateTimeCommand { get; }


        public EntryViewModel ParentEntryViewModel { get; }
        public EntryStateViewModel(EntryState entryState, EntryViewModel parentEntryViewModel, IDialogViewModel dialogViewModel) : base(dialogViewModel)
        {
            EntryState = entryState;
            ParentEntryViewModel = parentEntryViewModel;

            _fieldGroupsViewModels = new();
            FieldGroupsViewModels = new ReadOnlyObservableCollection<FieldGroupViewModel>(_fieldGroupsViewModels);
            var dirs = ParentEntryViewModel.GetAllParentDirectoryViewModels();
            dirs.Reverse();
            foreach (var dir in dirs)
            {
                var fg = EntryState.Fields.Find(el => el.BasedOn == dir.Directory.StatesTemplate);

                if (fg != null)
                    _fieldGroupsViewModels.Add(new FieldGroupViewModel(fg, dir.StateTemplateViewModel, DialogViewModel));
                else _fieldGroupsViewModels.Add(new FieldGroupViewModel(dir.StateTemplateViewModel, DialogViewModel));
            }

            ChangeDateTimeCommand = new(ChangeDateTime);
        }


        private void ChangeDateTime()
        {
            DateTimeInputWindowViewModel vm = AskForDateTime(DateTime, "Изменение временной метки");

            if (vm.DialogResult == true)
            {
                if (ParentEntryViewModel.StateViewModels.Any(el => el.DateTime == vm.DateTime))
                    ShowErrorMessage("Состояние с такой временной меткой уже существует");
                else
                    DateTime = vm.DateTime;
            }
        }
    }
}
