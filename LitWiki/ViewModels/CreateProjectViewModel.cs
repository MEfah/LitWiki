using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using LitWiki.Models;
using LitWiki.Tools;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LitWiki.ViewModels
{
    public class CreateProjectWindowViewModel : ViewModelBase, IModalDialogViewModel
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    AcceptCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(Name));
                }
            }
        }


        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                if (value != path)
                {
                    path = value;
                    AcceptCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(Path));
                }
            }
        }


        public bool? DialogResult { get; private set; } = false;


        public ProjectMetadata? CreatedProject { get; private set; } = null;


        public RelayCommand CancelCommand { get; }


        public RelayCommand AcceptCommand { get; }


        public RelayCommand PickFolderCommand { get; }


        private readonly IDialogService _dialogService;
        private readonly IFileService _fileService;
        public CreateProjectWindowViewModel(IDialogService dialogService, IFileService fileService)
        {
            _fileService = fileService;
            _dialogService = dialogService;

            CancelCommand = new RelayCommand(Cancel);
            AcceptCommand = new RelayCommand(Accept, CanAccept);
            PickFolderCommand = new RelayCommand(PickFolder);
        }


        private void Cancel()
        {
            DialogResult = false;
            _dialogService.Close(this);
        }

        private void Accept()
        {
            if (!System.IO.Directory.Exists(Path))
            {
                MessageBoxSettings settings = new();
                settings.MessageBoxText = "Папка не существует";
                settings.Caption = "Ошибка";
                settings.Button = MessageBoxButton.OK;
                settings.Icon = MessageBoxImage.Error;
                _dialogService.ShowMessageBox(this, settings);
                return;
            }

            if (File.Exists(Path + "/" + Name + _fileService.GetFileExtension()))
            {
                MessageBoxSettings settings = new();
                settings.MessageBoxText = "В папке уже содержится файл с указанным названием";
                settings.Caption = "Ошибка";
                settings.Button = MessageBoxButton.OK;
                settings.Icon = MessageBoxImage.Error;
                _dialogService.ShowMessageBox(this, settings);
                return;
            }

            CreatedProject = new ProjectMetadata()
            {
                Name = Name,
                FolderPath = Path,
                CreationDate = DateTime.Now,
                LastEditedDate = DateTime.Now
            };

            DialogResult = true;
            _dialogService.Close(this);
        }

        private bool CanAccept()
        {
            return Name != string.Empty && Path != string.Empty && Path != null && Name != null;
        }

        private void PickFolder()
        {
            FolderBrowserDialogSettings settings = new();

            if(_dialogService.ShowFolderBrowserDialog(this, settings) == true)
                Path = settings.SelectedPath;
        }
    }
}
