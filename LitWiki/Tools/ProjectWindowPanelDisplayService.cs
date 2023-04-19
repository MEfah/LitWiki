using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.Tools
{
    public interface IProjectWindowPanelDisplayService
    {
        IProjectWindowPanelViewModel DisplayedPanel { get; set; }
    }

    public interface IProjectWindowPanelViewModel
    {
        bool IsOpened { get; set; }
    }

    class ProjectWindowPanelDisplayService : IProjectWindowPanelDisplayService
    {
        private IProjectWindowPanelViewModel? _displayedPanel;
        public IProjectWindowPanelViewModel? DisplayedPanel
        {
            get { return _displayedPanel; }
            set
            {
                if(value != _displayedPanel)
                {
                    var oldValue = _displayedPanel;
                    _displayedPanel = value;
                    DisplayedPanelChanged?.Invoke(this, new ValueChangedEventArgs<IProjectWindowPanelViewModel?>(oldValue, value));
                }
            }
        }

        public event ValueChangedEventHandler<IProjectWindowPanelViewModel?>? DisplayedPanelChanged;
    }
}
