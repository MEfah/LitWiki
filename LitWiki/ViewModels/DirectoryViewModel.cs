using CommunityToolkit.Mvvm.Input;
using LitWiki.Models;
using LitWiki.Tools;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LitWiki.ViewModels
{
    // TODO: Move interfaces in separate file
    // TODO: Reduce inheritance somehow

    public interface IGroupViewModel : IGrouppableViewModel
    {
        ReadOnlyObservableCollection<IHierarchyElementViewModel> ChildViewModels { get; }
        SortingStrategyViewModel SortingStrategyViewModel { get; }


        public RelayCommand<IHierarchyElementViewModel> RemoveChildCommand { get; }


        public void Add(IHierarchyElementViewModel element);
        public void AddRange(params IHierarchyElementViewModel[] element);
        public void Remove(IHierarchyElementViewModel element);
    }

    public interface IGrouppableViewModel
    {
        IGroupViewModel? ParentViewModel { get; }
    }

    public interface IDialogViewModel : INotifyPropertyChanged
    {
        public IDialogService DialogService { get; }
    }

    public interface IDialogDependentViewModel
    {
        IDialogViewModel DialogViewModel { get; }
    }

    public class DirectoryViewModel : HierarchyGroupViewModel
    {
        public Directory Directory { get; set; }
        public TemplateViewModel EntryTemplateViewModel { get; }
        public TemplateViewModel StateTemplateViewModel { get; }


        public override string Name
        {
            get { return Directory.Name; }
            set
            {
                if (Directory.Name != value)
                {
                    Directory.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }


        private GroupingType _groupingType;
        public GroupingType GroupingType
        {
            get { return _groupingType; }
            set
            {
                if (_groupingType != value)
                {
                    _groupingType = value;
                    RemoveGroups();
                    AddGroupCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(GroupingType));
                }
            }
        }
        public event EventHandler<GroupingType> GroupingTypeChanged;


        public RelayCommand AddDirectoryCommand { get; }
        public RelayCommand AddEntryCommand { get; }
        public RelayCommand AddGroupCommand { get; }
        

        public DirectoryViewModel(Directory directory, IGroupViewModel parentViewModel, IDialogViewModel dialogViewModel) : base(dialogViewModel, parentViewModel)
        {
            Directory = directory;
            EntryTemplateViewModel = new(directory.EntriesTemplate, dialogViewModel, this);
            StateTemplateViewModel = new(directory.StatesTemplate, dialogViewModel, this);


            ItemsCollectionView.CustomSort = new DelegateComparer((obj1, obj2) =>
            {
                DirectoryViewModel? dir1 = obj1 as DirectoryViewModel;
                DirectoryViewModel? dir2 = obj2 as DirectoryViewModel;

                if (dir1 != null && dir2 != null)
                    return dir1.Name.CompareTo(dir2.Name);
                else if (dir1 == null && dir2 == null)
                {
                    IHierarchyElementViewModel el1 = (obj1 as IHierarchyElementViewModel)!;
                    IHierarchyElementViewModel el2 = (obj2 as IHierarchyElementViewModel)!;

                    return el1.Name.CompareTo(el2.Name);
                }
                else if(dir1 == null)
                    return 1;
                else if (dir2 == null)
                    return -1;
                return 0;
            });


            SortingStrategyViewModel.SortingTypeChanged += SortingStrategyViewModel_SortingTypeChanged;
            SortingStrategyViewModel.TemplateFieldViewModelChanged += SortingStrategyViewModel_TemplateFieldViewModelChanged;

            RemoveChildCommand = new RelayCommand<IHierarchyElementViewModel>(RemoveChild);
            AddDirectoryCommand = new RelayCommand(AddDirectory);
            AddEntryCommand = new RelayCommand(AddEntry);
            AddGroupCommand = new RelayCommand(AddGroup, CanAddGroup);
        }


        public ICollection<EntryViewModel> GetChildEntryViewModels()
        {
            List<EntryViewModel> list = new();

            foreach(var child in ChildViewModels)
            {
                if (child is IGroupViewModel group)
                {
                    List<IGroupViewModel> groups = new() { group };

                    while (groups.Count > 0)
                    {
                        foreach (var el in groups[0].ChildViewModels)
                        {
                            if (el is IGroupViewModel childGroup)
                                groups.Add(childGroup);
                            else if (el is EntryViewModel childEntry)
                                list.Add(childEntry);
                        }

                        groups.RemoveAt(0);
                    }
                }
                else if (child is EntryViewModel entry)
                    list.Add(entry);
            }

            return list;
        }

        public DirectoryViewModel? FindDirectory(string name)
        {
            foreach (var child in ChildViewModels)
                if (child is DirectoryViewModel dirVM)
                {
                    if (dirVM.Name == name)
                        return dirVM;

                    var dir = dirVM.FindDirectory(name);
                    if(dir != null) 
                        return dir;
                }

            return null;
        }



        private void RemoveGroups()
        {
            /*if(GroupingType == GroupingType.ByGroups)
            {
                List<EntryViewModel> toAdd = new();
                List<EntryViewModel> entries = new();

                foreach(var child in ChildViewModels)
                    if(child is EntryViewModel entry)
                        entries.Add(entry);

                while(entries.Count > 0)
                {
                    foreach(var entry in entries[0].ChildViewModels)
                        if(entry is EntryViewModel childEntry)
                        {
                            toAdd.Add(childEntry);
                            entries.Add(childEntry);
                        }

                    entries[0].Clear();
                    entries.RemoveAt(0);
                }

                AddRange(toAdd.ToArray());
            }
            else
            {
                List<EntryViewModel> toAdd = new();
                List<GroupViewModel> groups = new();

                foreach (var child in ChildViewModels)
                    if (child is GroupViewModel entry)
                        groups.Add(entry);

                foreach(var group in groups)
                    Remove(group);

                while (groups.Count > 0)
                {
                    foreach (var item in groups[0].ChildViewModels)
                        if (item is GroupViewModel childGroup)
                            groups.Add(childGroup);
                        else if(item is EntryViewModel entry)
                            toAdd.Add(entry);

                    groups.RemoveAt(0);
                }

                AddRange(toAdd.ToArray());
            }*/
        }

        private void SortingStrategyViewModel_SortingTypeChanged(object? sender, SortingType e)
        {
            SortChildViewModels();
        }

        private void SortingStrategyViewModel_TemplateFieldViewModelChanged(object? sender, TemplateFieldViewModel? e)
        {
            SortChildViewModels();
        }

        private void SortChildViewModels()
        {
            //_childViewModels = SortingStrategyViewModel.Sort(this);
            //ChildViewModels = new ReadOnlyObservableCollection<IHierarchyElementViewModel>(_childViewModels);
        }


        private void AddDirectory()
        {
            RenameWindowViewModel renameWindowViewModel = AskForName();
            HierarchyViewModel hierarchyViewModel = GetHierarchyViewModel();

            if (renameWindowViewModel.DialogResult == true)
            {
                if (!hierarchyViewModel.HasNameReserved(renameWindowViewModel.Name))
                {
                    Directory dir = new Directory() { Name = renameWindowViewModel.Name };
                    DirectoryViewModel dirVM = new(dir, this, DialogViewModel);
                    Add(dirVM);
                }
                else
                    ShowErrorMessage("Справочник или запись с таким названием уже существуют");
            }
        }

        private void AddEntry()
        {
            RenameWindowViewModel renameWindowViewModel = AskForName();

            if (renameWindowViewModel.DialogResult == true)
            {
                if (!GetHierarchyViewModel().HasNameReserved(renameWindowViewModel.Name))
                {
                    Entry entry = new Entry() { Name = renameWindowViewModel.Name };
                    EntryViewModel entryVM = new(entry, this, DialogViewModel);
                    Add(entryVM);
                }
                else
                    ShowErrorMessage("Справочник или запись с таким названием уже существуют");
            }
        }

        private void AddGroup()
        {
            RenameWindowViewModel renameWindowViewModel = AskForName();

            if (renameWindowViewModel.DialogResult == true)
            {
                if (ParentViewModel.ChildViewModels.Any(el => el.Name == renameWindowViewModel.Name && el is GroupViewModel))
                    ShowErrorMessage("Группа с таким названием уже существует");
                else
                {
                    Group group = new Group() { Name = renameWindowViewModel.Name };
                    GroupViewModel groupVM = new GroupViewModel(group, this, DialogViewModel);
                    Add(groupVM);
                }
            }
        }

        private bool CanAddGroup()
        {
            return GroupingType == GroupingType.ByGroups;
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
    }
}
