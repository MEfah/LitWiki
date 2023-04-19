using CommunityToolkit.Mvvm.Input;
using LitWiki.Models;
using LitWiki.Tools;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Xceed.Wpf.Toolkit;

namespace LitWiki.ViewModels
{
    public abstract class HierarchyGroupViewModel : HierarchyElementViewModel, IGroupViewModel
    {
        public abstract override string Name { get; set; }


        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if(_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }


        protected ObservableCollection<IHierarchyElementViewModel> _childViewModels;
        public ReadOnlyObservableCollection<IHierarchyElementViewModel> ChildViewModels { get; }



        public ListCollectionView ItemsCollectionView { get; private set; }


        public SortingStrategyViewModel SortingStrategyViewModel { get; } = new();


        public RelayCommand<IHierarchyElementViewModel> RemoveChildCommand { get; protected set; }

        

        protected HierarchyGroupViewModel(IDialogViewModel dialogViewModel, IGroupViewModel parentViewModel)
            : base(dialogViewModel, parentViewModel)
        {
            _childViewModels = new ObservableCollection<IHierarchyElementViewModel>();
            ChildViewModels = new(_childViewModels);

            ItemsCollectionView = (CollectionViewSource.GetDefaultView(ChildViewModels) as ListCollectionView)!;
            ItemsCollectionView.CustomSort = new DelegateComparer((obj1, obj2) =>
            {
                IHierarchyElementViewModel el1 = (obj1 as IHierarchyElementViewModel)!;
                IHierarchyElementViewModel el2 = (obj2 as IHierarchyElementViewModel)!;

                return el1.Name.CompareTo(el2.Name);
            });

            RemoveChildCommand = new RelayCommand<IHierarchyElementViewModel>(RemoveChild);
        }

        protected HierarchyGroupViewModel(IDialogViewModel dialogViewModel) : base(dialogViewModel)
        {
            _childViewModels = new ObservableCollection<IHierarchyElementViewModel>();
            ChildViewModels = new(_childViewModels);

            ItemsCollectionView = (CollectionViewSource.GetDefaultView(ChildViewModels) as ListCollectionView)!;
            ItemsCollectionView.CustomSort = new DelegateComparer((obj1, obj2) =>
            {
                IHierarchyElementViewModel el1 = (obj1 as IHierarchyElementViewModel)!;
                IHierarchyElementViewModel el2 = (obj2 as IHierarchyElementViewModel)!;

                return el1.Name.CompareTo(el2.Name);
            });


            RemoveChildCommand = new RelayCommand<IHierarchyElementViewModel>(RemoveChild);
        }


        public void Add(IHierarchyElementViewModel child)
        {
            HierarchyViewModel hierarchyViewModel = GetHierarchyViewModel();
            if (child is EntryViewModel or DirectoryViewModel && !hierarchyViewModel.HasNameReserved(child.Name))
            {
                hierarchyViewModel.ReserveName(child.Name);
                if (child is DirectoryViewModel dirVM)
                {
                    hierarchyViewModel.AddDirectoryType(dirVM);
                }
            }


            _childViewModels.Add(child);
            IsExpanded = true;
        }

        public void AddRange(params IHierarchyElementViewModel[] children)
        {
            foreach (var child in children)
                Add(child);
        }

        public void Remove(IHierarchyElementViewModel child)
        {
            HierarchyViewModel hierarchyViewModel = GetHierarchyViewModel();

            if (child is IGroupViewModel group)
            {
                List<IGroupViewModel> groups = new() { group };

                if (group is DirectoryViewModel dir)
                {
                    hierarchyViewModel.RemoveNameFromReserved(dir.Name);
                    hierarchyViewModel.RemoveDirectoryType(dir);
                }

                while (groups.Count > 0)
                {
                    foreach (var el in groups[0].ChildViewModels)
                    {
                        if (el is IGroupViewModel childGroup)
                        {
                            groups.Add(childGroup);

                            if (el is DirectoryViewModel dirVM)
                            {
                                hierarchyViewModel.RemoveNameFromReserved(el.Name);
                                hierarchyViewModel.RemoveDirectoryType(dirVM);
                            }
                        }
                        else if (el is EntryViewModel)
                            hierarchyViewModel.RemoveNameFromReserved(el.Name);
                    }

                    groups.RemoveAt(0);
                }
            }
            else if(child is EntryViewModel entry)
                hierarchyViewModel.RemoveNameFromReserved(entry.Name);

            _childViewModels.Remove(child);
            IsExpanded = true;


        }

        public bool FilterByName(string name)
        {
            IsExpanded = true;
            if (name == "")
            {
                ItemsCollectionView.Filter = (el) => true;
                foreach(var child in ChildViewModels)
                    if(child is HierarchyGroupViewModel groupVM)
                        groupVM.FilterByName(name);
            }
            else
            {
                ItemsCollectionView.Filter = (el) =>
                {
                    return el is HierarchyGroupViewModel groupVM && groupVM.FilterByName(name) ||
                        el is IHierarchyElementViewModel hierarchyElementViewModel && hierarchyElementViewModel.Name.ToLower().Contains(name.ToLower());
                };
            }

            ItemsCollectionView.Refresh();

            if (name != "")
                foreach (var item in ItemsCollectionView)
                    if (item is HierarchyGroupViewModel groupVM)
                        groupVM.IsExpanded = true;

            return !ItemsCollectionView.IsEmpty;
        }



        protected void RemoveChild(IHierarchyElementViewModel? child)
        {
            if (child == null)
                return;

            // Dont need to remove every child, the root is enough
            if (Confirm("Удалённый элемент невозможно будет вернуть. Выполнить операцию?"))
                Remove(child);
        }
    }
}
