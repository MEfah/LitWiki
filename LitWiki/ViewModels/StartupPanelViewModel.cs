using LitWiki.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Windows.Shapes;
using System.Xml.Serialization;
using LitWiki.Tools;
using System.Windows;
using System.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System.Xml.Linq;

namespace LitWiki.ViewModels
{
    class StartupPanelViewModel : ViewModelBase
    {
        // TODO: DependencyInjection for FileService that is used to save and load projects!!!!!!
        // TODO: Maybe change all XmlFileServices in the project to DataContractFileServices?

        // TODO: Create DialogService helper to open common dialogs with preset settings

        public ObservableCollection<ProjectMetadata> RecentProjects { get; private set; } = new();


        // When checked, selecting recentProject in the list will delete it
        private bool _isDeletingOnSelect;
        public bool IsDeletingOnSelect
        {
            get { return _isDeletingOnSelect; }
            set
            {
                if(_isDeletingOnSelect != value)
                {
                    _isDeletingOnSelect = value;
                    OnPropertyChanged(nameof(IsDeletingOnSelect));
                }
            }
        }


        public RelayCommand LoadRecentProjectsListCommand { get; }
        public RelayCommand SaveRecentProjectsListCommand { get; }
        public RelayCommand CreateProjectCommand { get; }
        public RelayCommand<ProjectMetadata> SelectProjectCommand { get; }
        public RelayCommand<ProjectMetadata> RenameProjectCommand { get; }
        public RelayCommand OpenFileCommand { get; }
        public RelayCommand OpenSettingsCommand { get; }


        // Used to send event to MainWindowViewModel to change page to settings
        public event EventHandler OpenSettings;
        // Used to close current window and open window with project
        public event EventHandler<Project> CloseAndOpenProject;


        private readonly IDialogService _dialogService;
        private readonly IFileService _projectFileService;
        private readonly IFileService _fileService;
        public StartupPanelViewModel(IDialogService dialogService, IFileService fileService)
        {
            _fileService = fileService;
            _dialogService = dialogService;
            _projectFileService = new DataContractFileService();

            LoadRecentProjectsListCommand = new RelayCommand(LoadRecentProjectsList);
            SaveRecentProjectsListCommand = new RelayCommand(SaveRecentProjectsList);
            CreateProjectCommand = new RelayCommand(CreateProject);
            SelectProjectCommand = new RelayCommand<ProjectMetadata>(SelectProject);
            RenameProjectCommand = new RelayCommand<ProjectMetadata>(RenameProject);
            OpenFileCommand = new RelayCommand(OpenFile);
            OpenSettingsCommand = new RelayCommand(() => OpenSettings?.Invoke(this, EventArgs.Empty));
        }



        private void CreateProject()
        {
            CreateProjectWindowViewModel createProjectViewModel = new(_dialogService, _projectFileService);
            _dialogService.ShowDialog(this, createProjectViewModel);
            

            if (createProjectViewModel.DialogResult == true && createProjectViewModel.CreatedProject != null)
            {
                RecentProjects.Add(createProjectViewModel.CreatedProject);
                OpenProject(createProjectViewModel.CreatedProject);
            }
        }

        private void RenameProject(ProjectMetadata? projectMetadata)
        {
            if (projectMetadata == null)
                return;

            RenameWindowViewModel renameViewModel = new RenameWindowViewModel(_dialogService);
            renameViewModel.Name = projectMetadata.Name;
            renameViewModel.WindowTitle = "Переименование проекта";
            _dialogService.ShowDialog(this, renameViewModel);
            
            if(renameViewModel.DialogResult == true)
            {
                ProjectMetadata newProjectMetadata = new() { 
                    Name = renameViewModel.Name,
                    FolderPath = projectMetadata.FolderPath,
                    CreationDate = projectMetadata.CreationDate,
                    LastEditedDate = projectMetadata.LastEditedDate,
                };

                if(File.Exists(newProjectMetadata.GetFullPath(_projectFileService)))
                {
                    MessageBoxSettings settings = new();
                    settings.MessageBoxText = "В папке уже содержится файл с указанным названием";
                    settings.Caption = "Ошибка";
                    settings.Button = MessageBoxButton.OK;
                    settings.Icon = MessageBoxImage.Error;
                    _dialogService.ShowMessageBox(this, settings);
                }
                else
                {
                    File.Move(projectMetadata.GetFullPath(_projectFileService), newProjectMetadata.GetFullPath(_projectFileService));
                    int index = RecentProjects.IndexOf(projectMetadata);
                    RecentProjects.RemoveAt(index);
                    RecentProjects.Insert(index, newProjectMetadata);
                }
            }
        }

        private void SelectProject(ProjectMetadata? projectMetadata)
        {
            if (IsDeletingOnSelect)
                DeleteProject(projectMetadata);
            else OpenProject(projectMetadata);
        }

        private void DeleteProject(ProjectMetadata? projectMetadata)
        {
            if (projectMetadata == null)
                return;

            if (File.Exists(projectMetadata.GetFullPath(_projectFileService)))
            {
                MessageBoxSettings settings = new();
                settings.Button = MessageBoxButton.YesNo;
                settings.MessageBoxText = "Вы уверены, что хотите удалить проект?";
                settings.Caption = "Внимание";
                settings.Icon = MessageBoxImage.Exclamation;

                if(_dialogService.ShowMessageBox(this, settings) == MessageBoxResult.Yes)
                {
                    File.Delete(projectMetadata.GetFullPath(_projectFileService));
                    RecentProjects.Remove(projectMetadata);
                }
            }
            else
                RecentProjects.Remove(projectMetadata);
        }

        private void OpenProject(ProjectMetadata? projectMetadata)
        {
            if (projectMetadata == null)
                return;

            Project? project;

            if (File.Exists(projectMetadata.GetFullPath(_projectFileService)))
            {
                project = Project.Load(projectMetadata, new DataContractFileService());
                if(project != null)
                    project.Metadata = projectMetadata;
            }
            else
                project = Project.CreateEmptyProject(projectMetadata, new DataContractFileService());



            if(project == null)
            {
                MessageBoxSettings settings = new();
                settings.MessageBoxText = "Произошла ошибка при открытии файла. Перезаписать файл?" +
                    " Все данные будут утеряны.";
                settings.Caption = "Ошибка";
                settings.Button = MessageBoxButton.YesNo;
                settings.Icon = MessageBoxImage.Error;

                if(_dialogService.ShowMessageBox(this, settings) == MessageBoxResult.Yes)
                {
                    project = new();
                    project = Project.CreateEmptyProject(projectMetadata, new DataContractFileService());
                }
            }

            RecentProjects.Move(RecentProjects.IndexOf(projectMetadata), 0);

            ProjectWindowViewModel projectViewModel = new ProjectWindowViewModel(_dialogService, _projectFileService, project);
            CloseAndOpenProject?.Invoke(this, project);
        }

        private void OpenFile()
        {
            OpenFileDialogSettings openFileDialogSettings = new()
            {
                Filter = _projectFileService.GetFileExtensionsDescription(),
                Title = "Выбор проекта",
                CheckFileExists = true
            };

            if(_dialogService.ShowOpenFileDialog(this, openFileDialogSettings) == true)
            {
                string path = openFileDialogSettings.FileName;
                ProjectMetadata projectMetadata = new ProjectMetadata();
                projectMetadata.SetFullPath(path);
                projectMetadata.CreationDate = File.GetCreationTime(path);
                projectMetadata.LastEditedDate = File.GetLastAccessTime(path);


                var res = RecentProjects.FirstOrDefault(el => el.GetFullPath(_projectFileService) == projectMetadata.GetFullPath(_projectFileService));
                // res is null if not in recentprojects
                if (res == null)
                {
                    res = projectMetadata;
                    RecentProjects.Add(res);
                }

                OpenProject(res);
            }
        }

        private void LoadRecentProjectsList()
        {
            Debug.WriteLine("Загружается список последних проектов");
            try
            {
                if (_fileService.Load(Settings.Current.LastProjectsPath, out Collection<ProjectMetadata>? res))
                {
                    Debug.WriteLine(Settings.Current.LastProjectsPath);
                    RecentProjects.Clear();

                    if(res != null)
                        foreach (var item in res)
                        {
                            if (File.Exists(item.GetFullPath(_projectFileService)))
                            {
                                item.CreationDate = File.GetCreationTime(item.GetFullPath(_projectFileService));
                                item.LastEditedDate = File.GetLastWriteTime(item.GetFullPath(_projectFileService));
                                RecentProjects.Add(item);
                            }
                        }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("При загрузке списка последних проектов произошла ошибка");
                Debug.WriteLine("При загрузке списка последних проектов произошла ошибка:\n" + ex.ToString());
                RecentProjects = new();
                //SaveRecentProjectsList();
            }
        }

        private void SaveRecentProjectsList()
        {
            Debug.WriteLine("Сохраняется список последних проектов");

            foreach(var item in RecentProjects)
            {
                Debug.WriteLine(item.GetFullPath(_fileService));
            }

            try
            {
                _fileService.Save(Settings.Current.LastProjectsPath, RecentProjects);
            }
            catch(Exception ex)
            {
                MessageBox.Show("При сохранении списка последних проектов возникла ошибка");
                Debug.WriteLine("При сохранении списка последних проектов возникла ошибка: " + ex);
            }
        }
    }
}
