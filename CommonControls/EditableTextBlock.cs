using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CommonControls
{
    [TemplatePart(Name = "EditTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "TextBlockLabel", Type = typeof(TextBlock))]
    public class EditableTextBlock : TextBox
    {
        public static readonly DependencyProperty IsEditedProperty = DependencyProperty.Register(
            "IsEdited",
            typeof(bool),
            typeof(EditableTextBlock),
            new PropertyMetadata(false, new PropertyChangedCallback(IsEditedChangedCallback)));
        public bool IsEdited
        {
            get { return (bool)GetValue(IsEditedProperty); }
            set { SetValue(IsEditedProperty, value); }
        }
        private static void IsEditedChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is EditableTextBlock etb)
            {
                bool value = (bool)args.NewValue;

                etb.UpdateEditState(value);
            }
        }
        private void UpdateEditState(bool value)
        {
            Debug.WriteLine("Editing - " + value);
            if (value)
            {
                if (EditTextBox != null)
                {
                    Debug.WriteLine("EditTextBox is visible now");
                    EditTextBox.Visibility = Visibility.Visible;
                    EditTextBox.Focus();
                    EditTextBox.SelectionStart = 0;
                    EditTextBox.SelectionLength = Text.Length;
                    Keyboard.Focus(EditTextBox);
                }

                if (TextBlockLabel != null)
                {
                    Debug.WriteLine("TextBlockLabel is collapsed now");
                    TextBlockLabel.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (TextBlockLabel != null)
                {
                    Debug.WriteLine("TextBlockLabel is visible now");
                    TextBlockLabel.Visibility = Visibility.Visible;
                }

                if (EditTextBox != null)
                {
                    Debug.WriteLine("EditTextBox is collapsed now");
                    EditTextBox.Visibility = Visibility.Collapsed;
                    Keyboard.ClearFocus();
                }
            }
        }


        private TextBox? _editTextBox;
        private TextBox? EditTextBox
        {
            get { return _editTextBox; }
            set
            {
                if(_editTextBox != null)
                {
                    _editTextBox.TextChanged -= _editTextBox_TextChanged;
                }

                _editTextBox = value;

                if(_editTextBox != null)
                {
                    _editTextBox.TextChanged += _editTextBox_TextChanged;
                }
            }
        }
        private void _editTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            // Cant be null because this event is only invoked by EditTextBox
            Text = EditTextBox!.Text;
        }



        private TextBlock? _textBlockLabel;
        private TextBlock? TextBlockLabel
        {
            get { return _textBlockLabel; }
            set
            {
                _textBlockLabel= value;
            }
        }



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            EditTextBox = GetTemplateChild("EditTextBox") as TextBox;
            TextBlockLabel = GetTemplateChild("TextBlockLabel") as TextBlock;
            UpdateEditState(IsEdited);

            GotFocus += EditableTextBlock_GotFocus;
            LostFocus += EditableTextBlock_LostFocus;
        }

        private void EditableTextBlock_LostFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Lost focus");

            if (EditTextBox != null && EditTextBox.IsFocused)
                Debug.WriteLine("But EditTextBox still is focused");
            else Debug.WriteLine("And EditTextBox is not focused either");
        }

        private void EditableTextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Got focus");
        }

        static EditableTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableTextBlock), new FrameworkPropertyMetadata(typeof(EditableTextBlock)));
        }
    }
}
