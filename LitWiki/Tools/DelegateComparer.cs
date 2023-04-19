using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.Tools
{
    public class DelegateComparer<T> : IComparer<T>
    {
        private Func<T?, T?, int> _compareStrategy;
        public DelegateComparer(Func<T?, T?, int> compareFunction)
        {
            _compareStrategy = compareFunction;
        }

        public int Compare(T? x, T? y)
        {
            return _compareStrategy.Invoke(x, y);
        }
    }

    public class DelegateComparer : IComparer
    {
        private Func<object?, object?, int> _compareStrategy;
        public DelegateComparer(Func<object?, object?, int> compareStrategy)
        {
            _compareStrategy = compareStrategy;
        }

        public int Compare(object? x, object? y)
        {
            return _compareStrategy.Invoke(x, y);
        }
    }
}
