using CommunityToolkit.Mvvm.Input;
using LitWiki.Models;
using LitWiki.Tools;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;
using LitWiki.Views;

namespace LitWiki.ViewModels
{
    public class ProjectWindowViewModel : ViewModelBase, IDialogViewModel
    {
        // TODO: Finish SaveAs, Open and Close commands


        public HierarchyViewModel HierarchyViewModel { get; }


        private IEditeableViewModel? _editedViewModel;
        public IEditeableViewModel? EditedViewModel
        {
            get { return _editedViewModel; }
            set
            {
                if(value != _editedViewModel)
                {
                    if (_editedViewModel != null)
                        _editedViewModel.IsEdited = false;

                    _editedViewModel = value;
                    OnPropertyChanged(nameof(EditedViewModel));

                    if(_editedViewModel != null)
                        _editedViewModel.IsEdited = true;
                }
            }
        }


        private bool _hideEmptyFields = false;


        public RelayCommand<object> OpenElementCommand { get; }


        // File menu commands
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand OpenCommand { get; }
        public RelayCommand CloseCommand { get; }

        // Edit menu commands
        public RelayCommand RenameCommand { get; }
        public RelayCommand GoToDirectoryCommand { get; }
        public RelayCommand GoToEntryCommand { get; }
        public RelayCommand FindStateCommand { get; }

        // View menu commands
        public RelayCommand SwitchEmptyFieldVisibilityCommand { get; }
        
        // Help menu commands
        public RelayCommand HelpCommand { get; }



        public IDialogService DialogService => _dialogService;
        private readonly IFileService _fileService;
        private readonly IDialogService _dialogService;
        public ProjectWindowViewModel(IDialogService dialogService, IFileService fileService, Project project)
        {
            _fileService = fileService;
            _dialogService = dialogService;

            HierarchyViewModel = new(project, fileService, this);

            #region SetCommands
            OpenElementCommand = new(OpenElement);

            SaveCommand = new(Save);
            SaveAsCommand = new(SaveAs);
            OpenCommand = new(Open);
            CloseCommand = new(Close);

            RenameCommand = new(Rename, CanRename);
            GoToDirectoryCommand = new(GoToDirectory);
            GoToEntryCommand = new(GoToEntry);
            FindStateCommand = new(FindState, CanFindState);

            SwitchEmptyFieldVisibilityCommand = new(SwitchEmptyFieldVisibility, CanSwitchEmptyFieldVisibility);

            HelpCommand = new(Help);
            #endregion

            OpenElement(new HelpPanelViewModel());
        }

        public void OpenElement(object? element)
        {
            if(EditedViewModel is EntryViewModel entryVM)
                entryVM.StateFound -= EntryVM_StateFound;
            else if(EditedViewModel is EntryStateViewModel stateVM)
                stateVM.ParentEntryViewModel.StateFound -= EntryVM_StateFound;

            RenameCommand.NotifyCanExecuteChanged();
            FindStateCommand.NotifyCanExecuteChanged();
            SwitchEmptyFieldVisibilityCommand.NotifyCanExecuteChanged();

            // Attach
            if (element is EntryViewModel elementEntryVM)
            {
                elementEntryVM.StateFound += EntryVM_StateFound;
                elementEntryVM.HideEmptyFields = _hideEmptyFields;
            }
            else if (element is EntryStateViewModel elementStateVM)
            {
                elementStateVM.ParentEntryViewModel.StateFound += EntryVM_StateFound;
                elementStateVM.ParentEntryViewModel.StatesCollectionView.MoveCurrentTo(elementStateVM.ParentEntryViewModel);
                elementStateVM.HideEmptyFields = _hideEmptyFields;
            }

            if (element == null)
                EditedViewModel = null;
            else if(element is IEditeableViewModel editeable and (EntryViewModel or EntryStateViewModel or DirectoryViewModel))
                EditedViewModel = editeable;
        }

        private void EntryVM_StateFound(object? sender, EntryStateViewModel e)
        {
            OpenElement(e);
        }



        private void Save()
        {
            HierarchyViewModel.UpdateProjectModelFromViewModels();
            foreach(var child in HierarchyViewModel.ChildViewModels)
            {
                Debug.WriteLine(child.Name);
                PrintTree((child as DirectoryViewModel)!, "  ");
            }
            //_fileService.Save(HierarchyViewModel.Project.Metadata.GetFullPath(_fileService), HierarchyViewModel.Project);
            DataContractFileService fs = new();
            Debug.WriteLine("PROJECT: " + HierarchyViewModel.Project.Metadata.FolderPath);
            fs.Save(HierarchyViewModel.Project.Metadata.GetFullPath(fs), HierarchyViewModel.Project);
        }

        private void PrintTree(HierarchyGroupViewModel group, string margin = "")
        {
            foreach(var child in group.ChildViewModels)
            {
                Debug.WriteLine(margin + child.Name);
                if (child is HierarchyGroupViewModel groupVM)
                    PrintTree(groupVM, margin + "  ");
            }
        }


        private void SaveAs()
        {

        }

        private void Open()
        {

        }

        private void Close()
        {

        }


        private void Rename()
        {
            (EditedViewModel as HierarchyElementViewModel).RenameCommand.Execute(null);
        }

        private bool CanRename()
        {
            return EditedViewModel is HierarchyElementViewModel;
        }

        private void GoToDirectory()
        {
            RenameWindowViewModel renameVM = new(_dialogService);
            renameVM.WindowTitle = "Поиск справочника";
            _dialogService.ShowDialog(this, renameVM);

            if(renameVM.DialogResult == true)
            {
                var res = HierarchyViewModel.FindDirectory(renameVM.Name);
                if (res == null)
                {
                    MessageBoxSettings settings = new();
                    settings.Caption = "Внимание";
                    settings.MessageBoxText = "Справочник с заданным названием не найдено";
                    settings.Button = MessageBoxButton.OK;
                    settings.Icon = MessageBoxImage.Exclamation;
                    _dialogService.ShowMessageBox(this, settings);
                }
                else
                    OpenElement(res);
            }
        }

        private void GoToEntry()
        {
            RenameWindowViewModel renameVM = new(_dialogService);
            renameVM.WindowTitle = "Поиск записи";
            _dialogService.ShowDialog(this, renameVM);

            if (renameVM.DialogResult == true)
            {
                var res = HierarchyViewModel.FindEntry(renameVM.Name);

                if (res == null)
                {
                    MessageBoxSettings settings = new();
                    settings.Caption = "Внимание";
                    settings.MessageBoxText = "Запись с заданным названием не найдено";
                    settings.Button = MessageBoxButton.OK;
                    settings.Icon = MessageBoxImage.Exclamation;
                    _dialogService.ShowMessageBox(this, settings);
                }
                else
                    OpenElement(res);
            }
        }

        private void FindState()
        {
            if (EditedViewModel is EntryViewModel entryVM)
                entryVM.FindStateCommand.Execute(null);
            else if (EditedViewModel is EntryStateViewModel entryStateVM)
                entryStateVM.ParentEntryViewModel.FindStateCommand.Execute(null);
        }

        private bool CanFindState()
        {
            return EditedViewModel is EntryViewModel || EditedViewModel is EntryStateViewModel;
        }


        private void SwitchEmptyFieldVisibility()
        {
            _hideEmptyFields = !_hideEmptyFields;
            Debug.WriteLine(_hideEmptyFields);
            if(EditedViewModel is IFieldGroupParent fieldGroupParent)
                fieldGroupParent.HideEmptyFields = _hideEmptyFields;
        }

        private bool CanSwitchEmptyFieldVisibility()
        {
            return EditedViewModel is EntryViewModel || EditedViewModel is EntryStateViewModel;
        }


        private void Help()
        {
            OpenElement(new HelpPanelViewModel());
        }
    }

    public interface IEditeableViewModel
    {
        public bool IsEdited { get; set; }
    }
}
