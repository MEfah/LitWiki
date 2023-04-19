using CommunityToolkit.Mvvm.Input;
using LitWiki.Models;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace LitWiki.ViewModels
{
    public class TemplateViewModel : DialogDependentViewModelBase 
    {
        public Template Template { get; set; }


        private ObservableCollection<TemplateFieldViewModel> _fields;
        public ReadOnlyObservableCollection<TemplateFieldViewModel> Fields { get; set; }


        public RelayCommand AddFieldCommand { get; }
        public RelayCommand<TemplateFieldViewModel> RemoveFieldCommand { get; }
        public DirectoryViewModel ParentDirectoryViewModel { get; }


        public event EventHandler<TemplateFieldViewModel> FieldAdded;
        public event EventHandler<int> FieldRemoved;
        public event EventHandler<FieldPositionChangedEventArgs> FieldPositionChanged;


        public TemplateViewModel(Template template, IDialogViewModel dialogViewModel, DirectoryViewModel parentDirectoryViewModel) : base(dialogViewModel)
        {
            Template = template;
            ParentDirectoryViewModel = parentDirectoryViewModel;

            _fields = new();
            Fields = new(_fields);
            foreach (var field in Template.Fields)
                _fields.Add(new TemplateFieldViewModel(field, dialogViewModel, this));


            AddFieldCommand = new(AddField);
            RemoveFieldCommand = new RelayCommand<TemplateFieldViewModel>(RemoveField);
        }


        public void AddField()
        {
            RenameWindowViewModel renameWindowViewModel = AskForName();

            if (renameWindowViewModel.DialogResult == true)
            {
                if (Fields.Any(el => el.Name == renameWindowViewModel.Name))
                    ShowErrorMessage("Поле с таким названием уже существует");
                else
                {
                    TemplateFieldViewModel field = new TemplateFieldViewModel(new TemplateField() { Name = renameWindowViewModel.Name,
                        Position = _fields.Count}, DialogViewModel, this);
                    _fields.Add(field);
                    FieldAdded?.Invoke(this, field);

                    // Notify previous last elements change position commands CanExecute so it would disable move down btn
                    if(_fields.Count > 1)
                        _fields[_fields.Count - 2].ChangePositionCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public void RemoveField(TemplateFieldViewModel? field)
        {
            if (field == null)
                throw new ArgumentNullException();

            int index = _fields.IndexOf(field);
            _fields.RemoveAt(index);
            FieldRemoved?.Invoke(this, index);

            if (index == Fields.Count && Fields.Count > 1)
                Fields[Fields.Count - 1].ChangePositionCommand.NotifyCanExecuteChanged();
            else
                for (int i = index; i < Fields.Count; i++)
                    Fields[i].Position--;
        }

        public void MoveFieldAndUpdatePositions(TemplateFieldViewModel? field, int from, int to)
        {
            if (field is null)
                throw new ArgumentNullException();
            
            if(Fields.Count < from || Fields.Count < to || from < 0 || to < 0)
                throw new ArgumentOutOfRangeException("Index was out of range. Must be non-negative and less than the " +
                    "size of the collection. ");

            if (Fields.Count < field.Position || Fields[from] != field)
                throw new Exception("Item in the collection and item passed as argument are not equal");

            if(from < to)
                for(int i = from + 1; i <= to; i++)
                    Fields[i].Position--;
            else
                for(int i = to; i >= from - 1; i--)
                    Fields[i].Position++;
            
            field.Position = to;

            _fields.Move(from, to);
            FieldPositionChanged?.Invoke(this, new FieldPositionChangedEventArgs() { From = from, To = to });
        }
    }

    public class FieldPositionChangedEventArgs
    {
        public int From { get; set; }
        public int To { get; set; }
    }
}
