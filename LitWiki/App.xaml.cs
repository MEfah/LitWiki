using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using LitWiki.Tools;
using LitWiki.ViewModels;
using LitWiki.Views;
using MvvmDialogs;

namespace LitWiki
{
    public partial class App : Application
    {
        // TODO: Create folder for images in resources (need to change image paths in views as well...)
        // TODO: Create DialogService helper to open common dialogs
        // TODO: Find and remove unused code (sortingstrategy and some tools and ...)
        // TODO: Check converters and behaviors if they are used or not
        // TODO: Use IList in models?
        // TODO: Some ViewModels still use ReadOnlyObservableCollections instead of CollectionViews, need to replace them
        // TODO: Figure out how to sync Models and ViewModels without redundant data
        // TODO: Create control that turns from textbox to textblock and use it for renaming
        // TODO: Create control that turns form dateinput to textblock and use it in 
        // TODO: Improve UI!!!
        // TODO: Change TreeView style to highilight whole item, change fill color to gray and highlight border on selection of item
        // TODO: Create TreeView with multiselect
        // TODO: Allow to move items in TreeView with drag and drop
        // TODO: Figure out what can be placed in settings page
        // TODO: Add shadow to edit directory/entry/state pages


        private IDialogService dialogService;
        private IFileService fileService;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow = new MainWindow();
            Configure();
            MainWindow.Show();
        }

        private void Configure()
        {
            dialogService = new DialogService();
            fileService = new XmlFileService();

            StartupPanelViewModel startupVM = new(dialogService, fileService);
            SettingsPanelViewModel settingsVM = new(dialogService); // Not used
            MainWindowViewModel mainVM = new(startupVM, settingsVM);

            // Need to close MainWindow through App, because MvvmDialogs can't close windows that in didn't open
            startupVM.CloseAndOpenProject += StartupVM_CloseAndOpenProject;

            MainWindow.DataContext = mainVM;
        }

        private void StartupVM_CloseAndOpenProject(object? sender, Models.Project e)
        {
            ProjectWindowViewModel projectWindowViewModel = new(dialogService, fileService, e);
            ProjectWindow projectWindow = new ProjectWindow();
            projectWindow.DataContext = projectWindowViewModel;
            projectWindow.Show();

            MainWindow.Close();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Save(Settings.Current);
        }
    }
}
