using CommunityToolkit.Mvvm.Input;
using LitWiki.Models;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace LitWiki.ViewModels
{
    public class GroupViewModel : HierarchyGroupViewModel
    {
        public Group Group { get; set; }


        public override string Name
        {
            get { return Group.Name; }
            set
            {
                if (Group.Name != value)
                {
                    Group.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

         
        public RelayCommand AddEntryCommand { get; }
        public RelayCommand AddGroupCommand { get; }



        public GroupViewModel(Group group, IGroupViewModel parentViewModel, IDialogViewModel dialogViewModel) : base(dialogViewModel, parentViewModel)
        {
            Group = group;

            AddEntryCommand = new(AddEntry);
            AddGroupCommand = new(AddGroup);
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

        protected override void Rename()
        {
            RenameWindowViewModel renameWindowViewModel = AskForName(Name);
            HierarchyViewModel projVM = GetHierarchyViewModel();

            if (renameWindowViewModel.DialogResult == true)
                if (ParentViewModel.ChildViewModels.Any(el => el.Name == renameWindowViewModel.Name && el is Group))
                    ShowErrorMessage("Группа с таким названием уже существует");
                else
                    Name = renameWindowViewModel.Name;
        }
    }
}
