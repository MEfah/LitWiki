using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.ViewModels
{
    public class RenameWindowViewModel : ViewModelBase, IModalDialogViewModel
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    AcceptCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged("Name");
                }
            }
        }


        private string _windowTitle = "Ввод названия";
        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                if (_windowTitle != value)
                {
                    _windowTitle = value;
                    OnPropertyChanged(nameof(WindowTitle));
                }
            }
        }


        public bool? DialogResult { get; private set; } = false;

        public RelayCommand CancelCommand { get; }
        public RelayCommand AcceptCommand { get; }


        private readonly IDialogService _dialogService;
        public RenameWindowViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            CancelCommand = new RelayCommand(Cancel);
            AcceptCommand = new RelayCommand(Accept, CanAccept);
        }


        private void Cancel()
        {
            DialogResult = false;

            _dialogService.Close(this);
        }

        private void Accept()
        {
            DialogResult = true;

            _dialogService.Close(this);
        }

        private bool CanAccept()
        {
            return Name != string.Empty && Name != null;
        }
    }
}
