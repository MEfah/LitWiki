using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using LitWiki.Tools;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LitWiki.ViewModels
{
    internal class SettingsPanelViewModel
    {
        public Settings Settings { get; set; }


        public RelayCommand ReturnToStartupPanelCommand { get; }


        public event EventHandler ReturnToStartup;


        private readonly IDialogService _dialogService;
        public SettingsPanelViewModel(IDialogService dialogService)
        {
            Settings = Settings.Current;
            _dialogService = dialogService;

            ReturnToStartupPanelCommand = new RelayCommand(ReturnToStartupPanel);
        }


        private void ReturnToStartupPanel()
        {
            ReturnToStartup?.Invoke(this, EventArgs.Empty);
        }
    }
}
