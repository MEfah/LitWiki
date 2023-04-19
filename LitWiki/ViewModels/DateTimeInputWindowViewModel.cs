using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.ViewModels
{
    public class DateTimeInputWindowViewModel : ViewModelBase, IModalDialogViewModel, IDialogDependentViewModel
    {
        private DateTime _dateTime;
        public DateTime DateTime
        {
            get { return _dateTime; }
            set
            {
                if(_dateTime != value)
                {
                    _dateTime = value;
                    OnPropertyChanged(nameof(DateTime));
                }
            }
        }


        private string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set
            {
                if(_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }


        public bool? DialogResult { get; private set; }


        public RelayCommand AcceptCommand { get; }
        public RelayCommand CancelCommand { get; }


        public IDialogViewModel DialogViewModel { get; }
        public DateTimeInputWindowViewModel(IDialogViewModel dialogViewModel)
        {
            DialogViewModel = dialogViewModel;

            AcceptCommand = new RelayCommand(Accept);
            CancelCommand = new RelayCommand(Cancel);
        }


        public void Accept()
        {
            DialogResult = true;
            DialogViewModel.DialogService.Close(this);
        }

        public void Cancel()
        {
            DialogResult = false;
            DialogViewModel.DialogService.Close(this);
        }
    }
}
