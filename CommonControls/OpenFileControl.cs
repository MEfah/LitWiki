using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommonControls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:CommonControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:CommonControls;assembly=CommonControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    /// 

    [TemplatePart(Name = "LabelTextBoxElement", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PathTextBoxElement", Type = typeof(TextBox))]
    [TemplatePart(Name = "BrowseFilesButtonElement", Type = typeof(Button))]
    public class OpenFileControl : Control
    {
        #region LabelTextProperty
        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
            "LabelText",
            typeof(string),
            typeof(OpenFileControl),
            new PropertyMetadata("", new PropertyChangedCallback(LabelTextChangedCallback)));
        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }
        private static void LabelTextChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {

        }
        #endregion


        #region PathProperty
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            "Path",
            typeof(string),
            typeof(OpenFileControl),
            new PropertyMetadata("", new PropertyChangedCallback(PathChangedCallback)));   
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }
        private static void PathChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is OpenFileControl ofc)
            {
                string path = (string)args.NewValue;
                ofc.OnPathChanged(new PathChangedEventArgs(PathChangedEvent, path));
            }
        }

        public static readonly RoutedEvent PathChangedEvent = EventManager.RegisterRoutedEvent(
            "PathChanged",
            RoutingStrategy.Bubble,
            typeof(PathChangedEventHandler),
            typeof(OpenFileControl));

        public event PathChangedEventHandler PathChanged
        {
            add { AddHandler(PathChangedEvent, value); }
            remove { RemoveHandler(PathChangedEvent, value);}
        }

        protected virtual void OnPathChanged(PathChangedEventArgs e)
        {
            RaiseEvent(e);
        }
        #endregion


        #region BroseFilesButton
        private Button? browseFilesButton;
        private Button? BrowseFilesButton
        {
            get { return browseFilesButton; }
            set
            {
                if(browseFilesButton != null)
                {
                    browseFilesButton.Click -= BrowseFilesButton_Click;
                }

                browseFilesButton = value;

                if(browseFilesButton != null)
                {
                    browseFilesButton.Click += BrowseFilesButton_Click;
                }
            }
        }

        private void BrowseFilesButton_Click(object sender, RoutedEventArgs e)
        {
            OnBrowseFiles(new RoutedEventArgs(BrowseFilesEvent));
            string result = "";

            switch (DialogMode)
            {
                case DialogMode.None:
                    return;

                case DialogMode.File:
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = FileFilter;
                    if(openFileDialog.ShowDialog() == true)
                        result = openFileDialog.FileName;
                    break;

                case DialogMode.Folder:
                    FolderPicker folderPicker = new FolderPicker();
                    if (folderPicker.ShowDialog() == true)
                        result = folderPicker.ResultPath;
                    break;
            }

            if (result != string.Empty)
                if(!BackSlash)
                    Path = result.Replace("\\", "/");
                else
                    Path = result;
        }

        public static readonly RoutedEvent BrowseFilesEvent = EventManager.RegisterRoutedEvent(
            "BrowseFiles",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(OpenFileControl));

        public event RoutedEventHandler BrowseFiles
        {
            add 
            {
                AddHandler(BrowseFilesEvent, value);
            }
            remove { RemoveHandler(BrowseFilesEvent, value); }
        }

        protected virtual void OnBrowseFiles(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        public static readonly DependencyProperty BrowseCommandProperty = DependencyProperty.Register(
            "BrowseCommand",
            typeof(ICommand),
            typeof(OpenFileControl),
            new PropertyMetadata(null, new PropertyChangedCallback(BrowseCommandChangedCallback)));
        private static void BrowseCommandChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            OpenFileControl control = obj as OpenFileControl;

            if(control != null && control.BrowseFilesButton != null)
                control.BrowseFilesButton.Command = args.NewValue as ICommand;
        }
        public ICommand BrowseCommand
        {
            get { return (ICommand)GetValue(BrowseCommandProperty); }
            set { SetValue(BrowseCommandProperty, value); }
        }
        #endregion


        public static readonly DependencyProperty DialogModeProperty = DependencyProperty.Register(
            "FileMode",
            typeof(DialogMode),
            typeof(OpenFileControl),
            new PropertyMetadata(DialogMode.None));
        public DialogMode DialogMode
        {
            get { return (DialogMode)GetValue(DialogModeProperty); }
            set { SetValue(DialogModeProperty, value); }
        }


        public static readonly DependencyProperty DialogOwnerProperty = DependencyProperty.Register(
            "DialogOwner",
            typeof(Window),
            typeof(OpenFileControl),
            new PropertyMetadata(null));
        public Window DialogOwner
        {
            get { return (Window)GetValue(DialogOwnerProperty); }
            set { SetValue(DialogOwnerProperty, value); }
        }


        public static readonly DependencyProperty FileFilterProperty = DependencyProperty.Register(
            "FileFilter",
            typeof(string),
            typeof(OpenFileControl),
            new PropertyMetadata(null));
        public string FileFilter
        {
            get { return (string)GetValue(FileFilterProperty); }
            set { SetValue(FileFilterProperty, value);}
        }


        public static readonly DependencyProperty BackSlashProperty = DependencyProperty.Register(
            "BackSlash",
            typeof(bool),
            typeof(OpenFileControl),
            new PropertyMetadata(false));
        public bool BackSlash
        {
            get { return (bool)GetValue(BackSlashProperty); }
            set { SetValue(BackSlashProperty, value); }
        }



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            BrowseFilesButton = GetTemplateChild("BrowseFilesButtonElement") as Button;
        }

        static OpenFileControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OpenFileControl), new FrameworkPropertyMetadata(typeof(OpenFileControl)));
        }
    }

    public delegate void PathChangedEventHandler(object sender, RoutedEventArgs e);

    public class PathChangedEventArgs : RoutedEventArgs
    {
        private string _path;

        public PathChangedEventArgs(RoutedEvent routedEvent, string path)
        {
            _path = path;
            RoutedEvent = routedEvent;
        }

        public string Path
        {
            get { return _path; }
        }
    }

    public enum DialogMode
    {
        None,
        Folder,
        File
    }
}
