using LitWiki.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        StartupPanelViewModel StartupPanelViewModel { get; set; }
        SettingsPanelViewModel SettingsPanelViewModel { get; set; }

        // Setting this property to StartupPanelVM or SettingsPanelVM can change pages to startupPanel and settingsPanel accordingly
        private object _currentViewModel;
        public object CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    OnPropertyChanged(nameof(CurrentViewModel));
                }
            }
        }


        public MainWindowViewModel(StartupPanelViewModel startupPanelViewModel, SettingsPanelViewModel settingsPanelViewModel)
        {
            StartupPanelViewModel = startupPanelViewModel;
            SettingsPanelViewModel = settingsPanelViewModel;

            // Subscribe to events to change pages
            // Prob not a good idea to use events for that but it's the fastest way
            StartupPanelViewModel.OpenSettings += StartupPanelViewModel_OpenSettings;
            SettingsPanelViewModel.ReturnToStartup += SettingsPanelViewModel_ReturnToStartup;

            // At first MainWindow shows startupPanel
            CurrentViewModel = StartupPanelViewModel;
        }

        private void SettingsPanelViewModel_ReturnToStartup(object? sender, EventArgs e)
        {
            CurrentViewModel = StartupPanelViewModel;
        }

        private void StartupPanelViewModel_OpenSettings(object? sender, EventArgs e)
        {
            CurrentViewModel = SettingsPanelViewModel;
        }
    }
}
