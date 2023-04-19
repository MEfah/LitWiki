using CommunityToolkit.Mvvm.Input;
using LitWiki.Models;
using LitWiki.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LitWiki.ViewModels
{
    public class FieldViewModel : ViewModelBase, IComparable<FieldViewModel>, IDialogDependentViewModel
    {
        // TODO: Find a better way to handle fields and completely remake this class
        // OR check TODOs lower at least


        private Field _field;
        public Field Field
        {
            get { return _field; }
            set
            {
                if (_field != value)
                {
                    _field = value;
                    //TemplateBase.TemplateField = value.TemplateField;
                    UpdateValueProxyByType(TemplateBase.Type);

                    // TODO: Move to separate method
                    // TODO: Let ValueProxy handle default values instead of setting them there
                    switch (TemplateBase.Type)
                    {
                        case FieldType.String:
                            ValueProxy.SetData(value.Value ?? "");
                            break;

                        case FieldType.Text:
                            ValueProxy.SetData(value.Value ?? "");
                            break;

                        case FieldType.Number:
                            ValueProxy.SetData(value.Value ?? 0);
                            break;

                        case FieldType.DateTime:
                            ValueProxy.SetData(value.Value ?? default(DateTime));
                            break;

                        case FieldType.Entry:
                            if(value.Value != null)
                                ValueProxy.SetData(value.Value);
                            break;

                        case FieldType.ListOfEntries:
                            if (value.Value != null)
                                ValueProxy.SetData(value.Value);
                            break;
                    }
                }
            }
        }


        public TemplateFieldViewModel TemplateBase { get; }


        public ValueProxy ValueProxy { get; private set; } = new TextValueProxy();


        public RelayCommand SelectEntryCommand { get; }


        public IDialogViewModel DialogViewModel { get; }
        public FieldViewModel(TemplateFieldViewModel basedOn, IDialogViewModel dialogViewModel)
        {
            TemplateBase = basedOn;

            UpdateValueProxyByType(TemplateBase.Type);
            TemplateBase.FieldTypeChanged += TemplateBase_FieldTypeChanged;

            Field = new();
            Field.TemplateField = basedOn.TemplateField;

            DialogViewModel = dialogViewModel;

            SelectEntryCommand = new(SelectEntry);
        }

        private void TemplateBase_FieldTypeChanged(object? sender, FieldType e)
        {
            UpdateValueProxyByType(e);
        }

        private void UpdateValueProxyByType(FieldType type)
        {
            // TODO: Do something to improve this part
            switch (type)
            {
                case FieldType.String:
                    ValueProxy = new TextValueProxy();
                    break;

                case FieldType.Text:
                    ValueProxy = new TextValueProxy();
                    break;

                case FieldType.Number:
                    ValueProxy = new NumberValueProxy();
                    break;

                case FieldType.DateTime:
                    ValueProxy = new DateTimeValueProxy();
                    break;

                case FieldType.Entry:
                    ValueProxy = new EntryValueProxy(this);
                    break;

                case FieldType.ListOfEntries:
                    ValueProxy = new ListOfEntriesValueProxy(this);
                    break;
            }
        }


        public void SelectEntry()
        {
            // TODO: Change exception message and type
            // TODO: Make method more readable

            if (TemplateBase.DirectoryViewModel == null)
                throw new Exception("!!!");

            bool multiple = false;
            List<EntryViewModel> selected = new();
            if (ValueProxy is EntryValueProxy entryProxy)
            {
                if (entryProxy.Entry != null)
                    selected.Add(entryProxy.Entry);

                SelectEntryWindowViewModel vm = new(TemplateBase.DirectoryViewModel.GetChildEntryViewModels(), selected, DialogViewModel, multiple);
                DialogViewModel.DialogService.ShowDialog(DialogViewModel, vm);

                if (vm.DialogResult == true)
                {
                    var sel = vm.GetSelected();

                    if (sel.Count == 0)
                        entryProxy.Entry = null;
                    else
                        entryProxy.Entry = sel[0];
                }
            }
            else if (ValueProxy is ListOfEntriesValueProxy listProxy)
            {
                multiple = true;
                foreach (var entry in listProxy.Entries)
                    selected.Add(entry);

                SelectEntryWindowViewModel vm = new(TemplateBase.DirectoryViewModel.GetChildEntryViewModels(), selected, DialogViewModel, multiple);
                DialogViewModel.DialogService.ShowDialog(DialogViewModel, vm);

                if (vm.DialogResult == true)
                    listProxy.Entries = new(vm.GetSelected());
            }
        }


        public int CompareTo(FieldViewModel? other)
        {
            return ValueProxy.Value.CompareTo(other.ValueProxy.Value);
        }
    }

    public abstract class ValueProxy : ViewModelBase
    {
        public abstract IComparable Value { get; }

        public abstract object GetData();
        public abstract void SetData(object value);
    }

    public class TextValueProxy : ValueProxy
    {
        private string _text = string.Empty;
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        public override IComparable Value { get => Text; }

        public override object GetData()
        {
            return Text;
        }

        public override void SetData(object data)
        {
            if (data is string s)
                Text = s;
        }
    }

    public class NumberValueProxy : ValueProxy
    {
        private double _number;
        public double Number
        {
            get { return _number; }
            set
            {
                if (_number != value)
                {
                    _number = value;
                    OnPropertyChanged(nameof(Number));
                }
            }
        }

        public override IComparable Value { get => Number; }

        public override object GetData()
        {
            return Number;
        }

        public override void SetData(object data)
        {
            if (data is double d)
                Number = d;
            else if (data is string s && double.TryParse(s, out double res))
                Number = res;
        }
    }

    public class DateTimeValueProxy : ValueProxy
    {
        private DateTime? _dateTime;
        public DateTime? DateTime
        {
            get { return _dateTime; }
            set
            {
                if (_dateTime != value)
                {
                    _dateTime = value;
                    OnPropertyChanged(nameof(DateTime));
                }
            }
        }

        public override IComparable Value { get => DateTime; }

        public override object GetData()
        {
            return DateTime;
        }

        public override void SetData(object data)
        {
            if (data is DateTime d)
                DateTime = d;
            else if (data is string s && System.DateTime.TryParse(s, out DateTime res))
                DateTime = res;
        }
    }


    public class EntryValueProxy : ValueProxy
    {
        public EntryViewModel? Entry
        {
            get
            {
                if(_field.TemplateBase.DirectoryViewModel != null && _entry != null)
                {
                    var res = _field.TemplateBase.DirectoryViewModel.GetChildEntryViewModels().FirstOrDefault(el => el.Entry == _entry);
                    return res;
                }

                return null;
            }
            set
            {
                if(value == null)
                {
                    if(_entry != null)
                    {
                        _entry = null;
                        OnPropertyChanged(nameof(Entry));
                    }
                    return;
                }

                if (_entry != value.Entry)
                {
                    _entry = value.Entry;
                    OnPropertyChanged(nameof(Entry));
                }
            }
        }


        private FieldViewModel _field;
        private Entry? _entry;
        public EntryValueProxy(FieldViewModel field)
        {
            _field = field;
        }


        public override IComparable Value { get => Entry != null ? Entry.Name : string.Empty; }

        public override object GetData()
        {
            return _entry;
        }

        public override void SetData(object data)
        {
            if (data is Entry entry)
                _entry = entry;
        }
    }

    public class ListOfEntriesValueProxy : ValueProxy
    {
        public ObservableCollection<EntryViewModel> Entries
        {
            get
            {
                if (_entries != null && _field.TemplateBase.DirectoryViewModel != null)
                {
                    var res = _field.TemplateBase.DirectoryViewModel.GetChildEntryViewModels();

                    return new(res.Where(el => _entries.Contains(el.Entry)));
                }
                else return new();
            }
            set
            {
                var items = value.Select(el => el.Entry).ToList();
                _entries = items;
                OnPropertyChanged(nameof(Entries));
            }
        }

        private FieldViewModel _field;
        private IEnumerable<Entry>? _entries;
        public ListOfEntriesValueProxy(FieldViewModel field)
        {
            _field = field;
        }

        public override IComparable Value { get => 0; }

        public override object GetData()
        {
            return _entries;
        }

        public override void SetData(object data)
        {
            Debug.WriteLine("DATA: " + data);
            if (data is IEnumerable<Entry> entries)
                _entries = entries;
        }
    }
}
