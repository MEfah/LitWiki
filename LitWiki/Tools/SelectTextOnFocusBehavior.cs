using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LitWiki.Tools
{
    class SelectTextOnFocusBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            AssociatedObject.TextChanged += AssociatedObject_TextChanged;
            AssociatedObject.PreviewTextInput += AssociatedObject_PreviewTextInput;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
            AssociatedObject.PreviewTextInput -= AssociatedObject_PreviewTextInput;
            base.OnDetaching();
        }

        private bool _selectedAfterGotFocus = false;
        private bool _textInput = false;

        private void AssociatedObject_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            _textInput = true;
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_selectedAfterGotFocus && !_textInput)
            {
                _selectedAfterGotFocus = true;
                AssociatedObject.SelectAll();
            }
        }

        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            _selectedAfterGotFocus = false;
        }

        public SelectTextOnFocusBehavior()
        {

        }
    }
}
