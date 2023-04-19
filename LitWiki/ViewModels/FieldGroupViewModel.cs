using LitWiki.Models;
using LitWiki.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.ViewModels
{
    public class FieldGroupViewModel : ViewModelBase, IDialogDependentViewModel
    {
        // TODO: Use CollectionView insead of ReadOnlyObservableCollection
        // TODO: Remove sync of field model and viewModel positions and let CollectionView handle sorting?


        public FieldGroup FieldGroup { get; set; }


        private ObservableCollection<FieldViewModel> _fields;
        public ReadOnlyObservableCollection<FieldViewModel> Fields { get; }



        private bool _showEmptyFields;
        public bool HideEmptyFields
        {
            get { return _showEmptyFields; }
            set
            {
                if(_showEmptyFields != value)
                {
                    _showEmptyFields = value;
                    OnPropertyChanged(nameof(HideEmptyFields));
                }
            }
        }



        public IDialogViewModel DialogViewModel { get; }
        public TemplateViewModel Base { get; }
        public FieldGroupViewModel(TemplateViewModel basedOn, IDialogViewModel dialogViewModel)
        {
            FieldGroup = new();

            _fields = new();
            Fields = new(_fields);
            DialogViewModel = dialogViewModel;

            Base = basedOn;
            Base.FieldAdded += Base_FieldAdded;
            Base.FieldRemoved += Base_FieldRemoved;
            Base.FieldPositionChanged += Base_FieldPositionChanged;

            // TODO: Probably have to check if index is valid or not, but couldn't break it while testing
            foreach (var item in Base.Fields)
                _fields.Add(new FieldViewModel(item, DialogViewModel));
        }
        public FieldGroupViewModel(FieldGroup fg, TemplateViewModel basedOn, IDialogViewModel dialogViewModel) : this(basedOn, dialogViewModel)
        {
            FieldGroup = fg;

            // TODO: Probably have to check if index is valid or not, but couldn't break it while testing
            foreach(var field in fg.Fields)
                _fields[field.TemplateField.Position].Field = field;
        }


        private void Base_FieldPositionChanged(object? sender, FieldPositionChangedEventArgs e)
        {
            _fields.Move(e.From, e.To);
        }

        private void Base_FieldRemoved(object? sender, int e)
        {
            _fields.RemoveAt(e);
        }

        private void Base_FieldAdded(object? sender, TemplateFieldViewModel e)
        {
            _fields.Add(new FieldViewModel(e, DialogViewModel));
        }
    }
}
