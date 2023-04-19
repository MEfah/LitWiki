using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LitWiki.Tools
{
    public class HierarchyContainerStyleSelector : StyleSelector
    {
        public List<StyleSelectionArgument> StyleSelectionArguments { get; set; } = new();

        public override Style SelectStyle(object item, DependencyObject container)
        {
            Type itemType = item.GetType();
            foreach(var argument in StyleSelectionArguments)
            {
                if(argument.Type.IsAssignableFrom(itemType))
                {
                    return argument.Style;
                }
            }
            /*            Style st = new Style();
                        st.TargetType = typeof(ListViewItem);
                        Setter backGroundSetter = new Setter();
                        backGroundSetter.Property = ListViewItem.BackgroundProperty;
                        ListView listView =
                            ItemsControl.ItemsControlFromItemContainer(container)
                              as ListView;
                        int index =
                            listView.ItemContainerGenerator.IndexFromContainer(container);
                        if (index % 2 == 0)
                        {
                            backGroundSetter.Value = Brushes.LightBlue;
                        }
                        else
                        {
                            backGroundSetter.Value = Brushes.Beige;
                        }
                        st.Setters.Add(backGroundSetter);
                        return st;*/
            return null;
        }
    }

    public class StyleSelectionArgument
    {
        public Type Type { get; set; }
        public Style Style { get; set; }
    }
}
