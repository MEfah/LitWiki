using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LitWiki.Models;
using LitWiki.ViewModels;

namespace LitWiki.Tools
{
    public class FieldTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is FieldViewModel fvm)
            {
                FieldType type = fvm.TemplateBase.Type;

                switch (type)
                {
                    case FieldType.String:
                        return element.FindResource("StringFieldTemplate") as DataTemplate;

                    case FieldType.Text:
                        return element.FindResource("TextFieldTemplate") as DataTemplate;

                    case FieldType.Number:
                        return element.FindResource("NumberFieldTemplate") as DataTemplate;

                    case FieldType.DateTime:
                        return element.FindResource("DateTimeFieldTemplate") as DataTemplate;

                    case FieldType.Entry:
                        return element.FindResource("EntryFieldTemplate") as DataTemplate;

                    case FieldType.ListOfEntries:
                        return element.FindResource("ListOfEntriesFieldTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }

    public class TaskListDataTemplateSelector : DataTemplateSelector
    {
        
    }
}
