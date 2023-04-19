using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using LitWiki.Models;
using LitWiki.Tools;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace LitWiki.ViewModels
{
    public class EntryViewModel : HierarchyElementViewModel, IEditeableViewModel, IFieldGroupParent
    {
        public Entry Entry { get; set; }


        private bool _showEmptyFields;
        public bool HideEmptyFields
        {
            get { return _showEmptyFields; }
            set
            {
                if (_showEmptyFields != value)
                {
                    _showEmptyFields = value;
                    OnPropertyChanged(nameof(HideEmptyFields));
                    foreach(var fieldGroup in FieldGroupsViewModels)
                        fieldGroup.HideEmptyFields = _showEmptyFields;
                }
            }
        }



        public override string Name
        {
            get { return Entry.Name; }
            set
            {
                if (Entry.Name != value)
                {
                    Entry.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }


        public FieldGroupViewModel Description { get; }
        private ObservableCollection<FieldGroupViewModel> _fieldGroupsViewModels;
        public ReadOnlyObservableCollection<FieldGroupViewModel> FieldGroupsViewModels { get; }


        private ObservableCollection<EntryStateViewModel> _stateViewModels;
        public ReadOnlyObservableCollection<EntryStateViewModel> StateViewModels { get; }

        public ListCollectionView StatesCollectionView { get; }


        public RelayCommand AddStateCommand { get; }
        public RelayCommand<EntryStateViewModel> RemoveStateCommand { get; }
        public RelayCommand FindStateCommand { get; }


        public event EventHandler<EntryStateViewModel>? StateFound;


        public EntryViewModel(Entry entry, IGroupViewModel parentViewModel, IDialogViewModel dialogViewModel) : base(dialogViewModel, parentViewModel)
        {
            Entry = entry;

            _stateViewModels = new();
            StateViewModels = new(_stateViewModels);

            #region FillStatesFromEntryModel

            foreach(var state in Entry.States)
                _stateViewModels.Add(new EntryStateViewModel(state, this, dialogViewModel));

            #endregion

            #region GetFieldGroupsFromDirectories

            _fieldGroupsViewModels = new();
            FieldGroupsViewModels = new ReadOnlyObservableCollection<FieldGroupViewModel>(_fieldGroupsViewModels);
            var dirs = GetAllParentDirectoryViewModels();
            dirs.Reverse();
            foreach (var dir in dirs)
            {
                var fg = entry.FieldGroups.Find(el => el.BasedOn == dir.EntryTemplateViewModel.Template);
                if(fg != null)
                    _fieldGroupsViewModels.Add(new FieldGroupViewModel(fg, dir.EntryTemplateViewModel, DialogViewModel));
                else _fieldGroupsViewModels.Add(new FieldGroupViewModel(dir.EntryTemplateViewModel, DialogViewModel));
            }

            #endregion

            StatesCollectionView = (CollectionViewSource.GetDefaultView(StateViewModels) as ListCollectionView)!;
            StatesCollectionView.CustomSort = new DelegateComparer((obj1, obj2) =>
            {
                EntryStateViewModel el1 = (obj1 as EntryStateViewModel)!;
                EntryStateViewModel el2 = (obj2 as EntryStateViewModel)!;

                return el1.DateTime.CompareTo(el2.DateTime);
            });

            AddStateCommand = new(AddState);
            RemoveStateCommand = new RelayCommand<EntryStateViewModel>(RemoveState);
            FindStateCommand = new(FindState);
        }


        protected override void Rename()
        {
            RenameWindowViewModel renameWindowViewModel = AskForName(Name);
            HierarchyViewModel projVM = GetHierarchyViewModel();

            if (renameWindowViewModel.DialogResult == true)
                if (projVM.HasNameReserved(renameWindowViewModel.Name))
                    ShowErrorMessage("Запись или справочник с таким названием уже существуют");
                else
                {
                    projVM.RemoveNameFromReserved(Name);
                    Name = renameWindowViewModel.Name;
                    projVM.ReserveName(Name);
                }
        }


        public DirectoryViewModel GetParentDirectoryViewModel()
        {
            if (ParentViewModel == null)
                throw new Exception();

            IGroupViewModel pointer = ParentViewModel;
            while (pointer is not DirectoryViewModel)
                pointer = pointer.ParentViewModel;

            return (DirectoryViewModel)pointer;
        }

        public List<DirectoryViewModel> GetAllParentDirectoryViewModels()
        {
            List<DirectoryViewModel> dirs = new();
            if (ParentViewModel == null)
                throw new Exception();

            IGroupViewModel pointer = ParentViewModel;
            while (pointer is not HierarchyViewModel)
            {
                if (pointer is DirectoryViewModel dir)
                    dirs.Add(dir);
                pointer = pointer.ParentViewModel;
            }

            return dirs;
        }



        private void AddState()
        {
            DateTimeInputWindowViewModel vm = new(DialogViewModel);
            vm.Title = "Новое состояние";
            if(StateViewModels.Count > 0)
                vm.DateTime = StateViewModels[StateViewModels.Count - 1].DateTime;
            DialogViewModel.DialogService.ShowDialog(DialogViewModel, vm);

            if(vm.DialogResult == true)
            {
                // Doing it this way to ensure that collection is always sorted
                EntryState state = new() { DateTime = vm.DateTime };
                EntryStateViewModel stateVM = new(state, this, DialogViewModel);

                if (_stateViewModels.Any(el => el.DateTime == vm.DateTime))
                    ShowErrorMessage("Состояние с такой временной меткой уже существует");
                else
                    _stateViewModels.Add(stateVM);
            }
        }

        private void RemoveState(EntryStateViewModel? state)
        {
            if (state == null)
                return;

            if (Confirm("Удалённое состояние невозможно будет вернуть. Выполнить операцию?"))
                _stateViewModels.Remove(state);
        }

        private void FindState()
        {
            var res = AskForDateTime(windowTitle: "Поиск состояния");

            if(res.DialogResult == true)
            {
                var state = StateViewModels.Where(el => el.DateTime <= res.DateTime).MaxBy(el => el.DateTime);
                if (state is null)
                    ShowErrorMessage("Для заданной временной метки не найдено ни одного состояния");
                else
                    StateFound?.Invoke(this, state);
            }
        }
    }
}
