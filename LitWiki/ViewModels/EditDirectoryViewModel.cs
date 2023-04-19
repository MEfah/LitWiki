using CommunityToolkit.Mvvm.Input;
using LitWiki.Models;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.ViewModels
{
    public class EditDirectoryViewModel : ViewModelBase, IEditeableViewModel
    {
        public DirectoryViewModel DirectoryViewModel { get; }


        public bool IsEdited { get; set; }


        private bool _hasChanges = false;
        public bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                if (_hasChanges != value)
                {
                    _hasChanges = value;
                    OnPropertyChanged(nameof(HasChanges));
                }
            }
        }


        private GroupingType _groupingType;
        public GroupingType GroupingType
        {
            get { return _groupingType; }
            set
            {
                if (_groupingType != value)
                {
                    _groupingType = value;
                    HasChanges = true;
                    OnPropertyChanged(nameof(GroupingType));

                    if (_groupingType == GroupingType.ByGroups && SortingStrategyViewModel.SortingType == SortingType.ByField)
                        SortingStrategyViewModel.SortingType = SortingType.ByName;
                }
            }
        }


        public SortingStrategyViewModel SortingStrategyViewModel { get; } = new();


        //public RelayCommand ApplyChangesCommand { get; }


        public EditDirectoryViewModel(DirectoryViewModel directoryViewModel)
        {
            DirectoryViewModel = directoryViewModel;
            GroupingType = DirectoryViewModel.GroupingType;
            SortingStrategyViewModel.TemplateFieldViewModel = DirectoryViewModel.SortingStrategyViewModel.TemplateFieldViewModel;
            SortingStrategyViewModel.SortingType = DirectoryViewModel.SortingStrategyViewModel.SortingType;

            SortingStrategyViewModel.TemplateFieldViewModelChanged += SortingStrategyViewModel_TemplateFieldViewModelChanged;
            SortingStrategyViewModel.SortingTypeChanged += SortingStrategyViewModel_SortingTypeChanged;

            //ApplyChangesCommand = new(ApplyChanges);
            HasChanges = false;
        }

        private void SortingStrategyViewModel_TemplateFieldViewModelChanged(object? sender, TemplateFieldViewModel e)
        {
            HasChanges = true;
        }

        private void SortingStrategyViewModel_SortingTypeChanged(object? sender, SortingType e)
        {
            HasChanges = true;
        }

        // Not used
        private void ApplyChanges()
        {
            /*if(DirectoryViewModel.GroupingType != GroupingType)
            {
                if(GroupingType == GroupingType.ByGroups)
                {
                    if (DirectoryViewModel.ChildViewModels.Any(el => el is EntryViewModel entry && entry.ChildViewModels.Count > 0))
                    {
                        if (Confirm("Справочник содержит группировку по записям. Если применить изменения, группировка будет утеряна. " +
                            "Вы уверены, что хотите продолжить?"))
                            SynchronizeWithDirVM();
                    }
                    else
                        SynchronizeWithDirVM();
                }
                else
                {
                    if (DirectoryViewModel.ChildViewModels.Any(el => el is GroupViewModel group && group.ChildViewModels.Count > 0))
                    {
                        if (Confirm("Справочник содержит группы. Если применить изменения, они будут расформированы. " +
                            "Вы уверены, что хотите продолжить?"))
                            SynchronizeWithDirVM();
                    }
                    else
                        SynchronizeWithDirVM();
                }
            }
            else
                SynchronizeWithDirVM();*/
        }
    }
}
