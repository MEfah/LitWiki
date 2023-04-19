using Accessibility;
using CommunityToolkit.Mvvm.Input;
using LitWiki.Models;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace LitWiki.ViewModels
{
    public class TemplateFieldViewModel : ViewModelBase, IDialogDependentViewModel
    {
        public TemplateField TemplateField { get; set; }


        public string Name
        {
            get { return TemplateField.Name; }
            set
            {
                if (TemplateField.Name != value)
                {
                    TemplateField.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }


        public FieldType Type
        {
            get { return TemplateField.FieldType; }
            set
            {
                if(TemplateField.FieldType != value)
                {
                    TemplateField.FieldType = value;
                    FieldTypeChanged?.Invoke(this, TemplateField.FieldType);
                    OnPropertyChanged(nameof(Type));
                }
            }
        }


        private DirectoryViewModel? _directoryViewModel;
        public DirectoryViewModel? DirectoryViewModel
        {
            get
            {
                var res = TemplateViewModel.ParentDirectoryViewModel.GetHierarchyViewModel().ExistingDirectories.FirstOrDefault(el => {
                    return el.Directory == TemplateField.EntrySource;
                });
                return res;
            }
            set
            {
                if(_directoryViewModel != value)
                {
                    _directoryViewModel = value;
                    if(_directoryViewModel != null)
                        TemplateField.EntrySource = _directoryViewModel.Directory;
                    else TemplateField.EntrySource = null;
                    OnPropertyChanged(nameof(DirectoryViewModel));
                }
            }
        }


        public int Position
        {
            get { return TemplateField.Position; }
            set
            {
                if(TemplateField.Position != value)
                {
                    TemplateField.Position = value;
                    ChangePositionCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(Position));
                }
            }
        }


        public event EventHandler<FieldType> FieldTypeChanged;


        public RelayCommand RenameCommand { get; }
        public RelayCommand<int> ChangePositionCommand { get; }


        public IDialogViewModel DialogViewModel { get; }
        public TemplateViewModel TemplateViewModel { get; }
        public TemplateFieldViewModel(TemplateField templateField, IDialogViewModel dialogViewModel, TemplateViewModel parent)
        {
            TemplateField = templateField;
            if(templateField.EntrySource!= null)
            {
                var res = parent.ParentDirectoryViewModel.GetHierarchyViewModel().ExistingDirectories.FirstOrDefault(el => {
                    return el.Directory == templateField.EntrySource;
                });
                if (res != null)
                    DirectoryViewModel = res;
            }

            DialogViewModel = dialogViewModel;
            TemplateViewModel = parent;

            RenameCommand = new RelayCommand(Rename);
            ChangePositionCommand = new RelayCommand<int>(ChangePosition, CanChangePosition);
        }

        public TemplateFieldViewModel(IDialogViewModel dialogViewModel, TemplateViewModel parent)
        {
            TemplateField = new();

            DialogViewModel = dialogViewModel;
            TemplateViewModel = parent;

            RenameCommand = new RelayCommand(Rename);
            ChangePositionCommand = new RelayCommand<int>(ChangePosition, CanChangePosition);
        }


        public void Rename()
        {
            RenameWindowViewModel renameWindowViewModel = AskForName();

            if (renameWindowViewModel.DialogResult == true)
                if (TemplateViewModel.Fields.Any(f => f.Name == renameWindowViewModel.Name))
                    ShowErrorMessage("Поле с таким названием уже существует");
                else
                    Name = renameWindowViewModel.Name;
        }

        public void ChangePosition(int newPosition)
        {
            TemplateViewModel.MoveFieldAndUpdatePositions(this, Position, newPosition);
        }

        public bool CanChangePosition(int newPosition)
        {
            return newPosition < TemplateViewModel.Fields.Count && newPosition >= 0;
        }


        protected void ShowErrorMessage(string message)
        {
            MessageBoxSettings settings = new();
            settings.Button = MessageBoxButton.OK;
            settings.Caption = "Ошибка";
            settings.MessageBoxText = message;
            settings.Icon = MessageBoxImage.Error;
            DialogViewModel.DialogService.ShowMessageBox(DialogViewModel, settings);
        }

        protected RenameWindowViewModel AskForName()
        {
            RenameWindowViewModel renameViewModel = new RenameWindowViewModel(DialogViewModel.DialogService);
            renameViewModel.Name = Name;
            DialogViewModel.DialogService.ShowDialog(DialogViewModel, renameViewModel);
            return renameViewModel;
        }
    }
}
