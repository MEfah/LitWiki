using CommunityToolkit.Mvvm.Input;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace LitWiki.ViewModels
{
    // TODO: Reduce inheritance somehow?


    public interface IHierarchyElementViewModel : INotifyPropertyChanged, IGrouppableViewModel
    {
        public string Name { get; set; }
        public HierarchyViewModel GetHierarchyViewModel();

        public RelayCommand RenameCommand { get; }
    }

    public abstract class DialogDependentViewModelBase : ViewModelBase, IDialogDependentViewModel
    {
        public IDialogViewModel DialogViewModel { get; }

        public DialogDependentViewModelBase(IDialogViewModel dialogViewModel)
        {
            DialogViewModel = dialogViewModel;
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

        protected RenameWindowViewModel AskForName(string name = "", string? windowTitle = null)
        {
            RenameWindowViewModel renameViewModel = new RenameWindowViewModel(DialogViewModel.DialogService);
            renameViewModel.Name = name;
            if (windowTitle != null)
                renameViewModel.WindowTitle = windowTitle;
            DialogViewModel.DialogService.ShowDialog(DialogViewModel, renameViewModel);
            return renameViewModel;
        }

        protected DateTimeInputWindowViewModel AskForDateTime(DateTime dateTime = default, string? windowTitle = null)
        {
            DateTimeInputWindowViewModel vm = new(DialogViewModel);
            if(windowTitle != null)
                vm.Title = windowTitle;
            vm.DateTime = dateTime;
            DialogViewModel.DialogService.ShowDialog(DialogViewModel, vm);
            return vm;
        }

        protected bool Confirm(string message)
        {
            MessageBoxSettings settings = new();
            settings.MessageBoxText = message;
            settings.Caption = "Внимание";
            settings.Button = MessageBoxButton.YesNo;
            settings.Icon = MessageBoxImage.Exclamation;
            return DialogViewModel.DialogService.ShowMessageBox(DialogViewModel, settings) == MessageBoxResult.Yes;
        }
    }

    public abstract class HierarchyElementViewModel : DialogDependentViewModelBase, IHierarchyElementViewModel, IEditeableViewModel
    {
        public abstract string Name { get; set; }


        private IGroupViewModel? _parentViewModel;
        public IGroupViewModel? ParentViewModel
        {
            get { return _parentViewModel; }
            set
            {
                _parentViewModel = value;
            }
        }

        public RelayCommand RenameCommand { get; }


        private bool _isEdited;
        public bool IsEdited
        {
            get { return _isEdited; }
            set
            {
                if(_isEdited != value)
                {
                    _isEdited = value;
                    OnPropertyChanged(nameof(IsEdited));
                }
            }
        }


        public HierarchyElementViewModel(IDialogViewModel dialogViewModel, IGroupViewModel parentViewModel) : base(dialogViewModel)
        {
            ParentViewModel = parentViewModel;

            RenameCommand = new RelayCommand(OnRename);
        }

        public HierarchyElementViewModel(IDialogViewModel dialogViewModel) : base(dialogViewModel)
        {
            RenameCommand = new RelayCommand(OnRename);
        }

        public HierarchyViewModel GetHierarchyViewModel()
        {
            if (ParentViewModel == null)
                throw new Exception();

            IGroupViewModel pointer = ParentViewModel;
            while (pointer is not HierarchyViewModel)
                pointer = pointer.ParentViewModel;
            return (HierarchyViewModel)pointer;
        }

        private void OnRename()
        {
            Rename();

            ParentViewModel.Remove(this);
            ParentViewModel.Add(this);
            IsEdited = true;
        }

        protected abstract void Rename();
    }
}
