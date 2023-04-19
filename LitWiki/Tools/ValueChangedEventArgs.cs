using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace LitWiki.Tools
{
    public delegate void ValueChangedEventHandler<T>(object sender, ValueChangedEventArgs<T> e);

    public class ValueChangedEventArgs<T>
    {
        public T OldValue { get; private set; }
        public T NewValue { get; private set; }

        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
