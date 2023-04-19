using CommunityToolkit.Mvvm.Input;
using LitWiki.Models;
using LitWiki.Tools;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;
using System.CodeDom;
using System.Windows.Data;

namespace LitWiki.ViewModels
{
    public class HierarchyViewModel : HierarchyGroupViewModel, IDialogDependentViewModel
    {
        public Project Project { get; private set; }


        public override string Name { get { return Project.Metadata.Name; } set { Project.Metadata.Name = value; } }


        private HashSet<string> _reservedNames = new();


        private ObservableCollection<DirectoryViewModel> _existingDirectories;
        public ReadOnlyObservableCollection<DirectoryViewModel> ExistingDirectories { get; }

        public ListCollectionView ExistingDirectoriesView { get; }

        // Need for hierarchy view to display functional project root element
        // Also potentially can be used to show multiple projects in project hierarchy
        private ObservableCollection<HierarchyViewModel> _rootViewModels;
        public ReadOnlyObservableCollection<HierarchyViewModel> RootViewModels { get; }


        public RelayCommand<string> FilterItemsByNameCommand { get; }
        public RelayCommand AddDirectoryCommand { get; }


        private readonly IFileService _fileService;
        public HierarchyViewModel(Project project, IFileService fileService, IDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            Project = project;
            _fileService = fileService;
            ParentViewModel = this;

            _existingDirectories = new();
            ExistingDirectories = new ReadOnlyObservableCollection<DirectoryViewModel>(_existingDirectories);

            ExistingDirectoriesView = (ListCollectionView)CollectionViewSource.GetDefaultView(ExistingDirectories);
            ExistingDirectoriesView.CustomSort = new DelegateComparer((obj1, obj2) =>
            {
                return (obj1 as DirectoryViewModel)!.Name.CompareTo((obj2 as DirectoryViewModel)!.Name);
            });

            _rootViewModels = new() { this };
            RootViewModels = new ReadOnlyObservableCollection<HierarchyViewModel>(_rootViewModels);

            GetChildViewModelsFromProject();

            FilterItemsByNameCommand = new(FilterItemsByName);
            AddDirectoryCommand = new(AddDirectory);
        }


        private void GetChildViewModelsFromProject()
        {
            _childViewModels.Clear();

            foreach (var dir in Project.Directories)
            {
                DirectoryViewModel dirVM = CreateDirectoryViewModel(dir, this);
                dirVM.AddRange(ConvertModelTreeToViewModelTree(dir, dirVM).ToArray());
                _childViewModels.Add(dirVM);
            }
        }

        private List<IHierarchyElementViewModel> ConvertModelTreeToViewModelTree(IGroup group, IGroupViewModel parent)
        {
            List<IHierarchyElementViewModel> viewModelTree = new List<IHierarchyElementViewModel>();

            foreach (var element in group.Elements)
            {
                if (element is Directory dir)
                {
                    DirectoryViewModel dirVM = CreateDirectoryViewModel(dir, parent);
                    dirVM.AddRange(ConvertModelTreeToViewModelTree(dir, dirVM).ToArray());
                    viewModelTree.Add(dirVM);
                }
                else if (element is Group g)
                {
                    GroupViewModel gVM = new(g, parent, DialogViewModel);
                    gVM.AddRange(ConvertModelTreeToViewModelTree(g, gVM).ToArray());
                    viewModelTree.Add(gVM);
                }
                else if (element is Entry e)
                {
                    EntryViewModel eVM = new(e, parent, DialogViewModel);
                    ReserveName(eVM.Name);
                    viewModelTree.Add(eVM);
                }
            }

            return viewModelTree;
        }

        private DirectoryViewModel CreateDirectoryViewModel(Directory directory, IGroupViewModel parent)
        {
            DirectoryViewModel dirVM = new(directory, parent, DialogViewModel);
            ReserveName(dirVM.Name);
            _existingDirectories.Add(dirVM);

            return dirVM;
        }



        public void UpdateProjectModelFromViewModels()
        {
            Project.Directories.Clear();

            foreach (var child in ChildViewModels)
            {
                DirectoryViewModel dirVM = (child as DirectoryViewModel)!;

                UpdateDirectoryFromViewModel(dirVM.Directory, dirVM);

                Project.Directories.Add(dirVM.Directory);
            }
        }

        private List<IGrouppable> ConvertViewModelTreeToModelTree(IGroupViewModel group, IGroup parent)
        {
            List<IGrouppable> modelTree = new List<IGrouppable>();

            foreach (var element in group.ChildViewModels)
            {
                switch (element)
                {
                    case DirectoryViewModel dirVM:
                        dirVM.Directory.ParentGroup = parent;
                        UpdateDirectoryFromViewModel(dirVM.Directory, dirVM);
                        modelTree.Add(dirVM.Directory);
                        break;

                    case GroupViewModel groupVM:
                        groupVM.Group.ParentGroup = parent;
                        UpdateGroupFromViewModel(groupVM.Group, groupVM);
                        modelTree.Add(groupVM.Group);
                        break;

                    case EntryViewModel entryVM:
                        entryVM.Entry.ParentGroup = parent;
                        UpdateEntryFromViewModel(entryVM.Entry, entryVM);
                        modelTree.Add(entryVM.Entry);
                        break;
                }
            }

            return modelTree;
        }

        private void UpdateDirectoryFromViewModel(Directory directory, DirectoryViewModel directoryVM)
        {
            // Set items
            directory.Elements.Clear();
            directory.Elements.AddRange(ConvertViewModelTreeToModelTree(directoryVM, directory));

            // Set entries template
            directory.EntriesTemplate = directoryVM.EntryTemplateViewModel.Template;
            directory.EntriesTemplate.Fields.Clear();
            foreach (var field in directoryVM.EntryTemplateViewModel.Fields)
            {
                directory.EntriesTemplate.Fields.Add(field.TemplateField);
            }

            directory.StatesTemplate = directoryVM.StateTemplateViewModel.Template;
            directory.StatesTemplate.Fields.Clear();
            foreach (var field in directoryVM.StateTemplateViewModel.Fields)
                directory.StatesTemplate.Fields.Add(field.TemplateField);
        }

        private void UpdateGroupFromViewModel(Group group, GroupViewModel groupVM)
        {
            group.Elements.Clear();
            group.Elements.AddRange(ConvertViewModelTreeToModelTree(groupVM, group));
        }

        private void UpdateEntryFromViewModel(Entry entry, EntryViewModel entryVM)
        {
            entry.FieldGroups.Clear();

            foreach(var fgVM in entryVM.FieldGroupsViewModels)
            {
                FieldGroup fg = new();
                fg.BasedOn = fgVM.Base.Template;
                foreach (var fVM in fgVM.Fields)
                {
                    fg.Fields.Add(fVM.Field);
                    fVM.Field.Value = fVM.ValueProxy.GetData();
                }
                entry.FieldGroups.Add(fg);
            }

            entry.States.Clear();
            foreach(var sVM in entryVM.StateViewModels)
            {
                entry.States.Add(sVM.EntryState);
                sVM.EntryState.Fields.Clear();

                foreach (var fgVM in sVM.FieldGroupsViewModels)
                {
                    FieldGroup fg = new();
                    fg.BasedOn = fgVM.Base.Template;
                    foreach (var fVM in fgVM.Fields)
                    {
                        fg.Fields.Add(fVM.Field);
                        fVM.Field.Value = fVM.ValueProxy.GetData();
                    }

                    sVM.EntryState.Fields.Add(fg);
                }
            }
        }




        public bool HasNameReserved(string name)
        {
            return _reservedNames.Contains(name);
        }

        public void ReserveName(string name)
        {
            _reservedNames.Add(name);
        }

        public void RemoveNameFromReserved(string name)
        {
            _reservedNames.Remove(name);
        }

        public EntryViewModel? FindEntry(string name)
        {
            List<HierarchyGroupViewModel> groups = new();
            name = name.ToLower();

            foreach (var child in ChildViewModels)
            {
                if(child is EntryViewModel entryVM && entryVM.Name.ToLower() == name)
                    return entryVM;
                else if(child is HierarchyGroupViewModel group)
                    groups.Add(group);
            }

            while(groups.Count > 0)
            {
                foreach(var child in groups[0].ChildViewModels)
                {
                    if (child is EntryViewModel entryVM && entryVM.Name.ToLower() == name)
                        return entryVM;
                    else if (child is HierarchyGroupViewModel group)
                        groups.Add(group);
                }

                groups.RemoveAt(0);
            }

            return null;
        }

        public DirectoryViewModel? FindDirectory(string name)
        {
            List<DirectoryViewModel> dirs = new();
            name = name.ToLower();
            foreach (var child in ChildViewModels)
            {
                if(child is DirectoryViewModel dir)
                {
                    if (dir.Name.ToLower() == name)
                        return dir;
                    else dirs.Add(dir);
                }
            }

            while (dirs.Count > 0)
            {
                foreach (var child in dirs[0].ChildViewModels)
                {
                    if (child is DirectoryViewModel dir)
                    {
                        if (dir.Name.ToLower() == name)
                            return dir;
                        else dirs.Add(dir);
                    }
                }

                dirs.RemoveAt(0);
            }

            return null;
        }


        public void AddDirectoryType(DirectoryViewModel directoryType)
        {
            _existingDirectories.Add(directoryType);
        }

        public void RemoveDirectoryType(DirectoryViewModel directoryViewModel)
        {
            _existingDirectories.Remove(directoryViewModel);
        }

        protected override void Rename()
        {
            
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

        private void FilterItemsByName(string? name)
        {
            name ??= "";
            FilterByName(name);
        }
    }
}
