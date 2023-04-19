using LitWiki.Models;
using LitWiki.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace LitWiki
{
    /// <summary>
    /// Class that contains settings for appplication
    /// Was intended to be editable, but currently is read only
    /// </summary>
    [Serializable]
    public class Settings
    {
        private static IFileService FileService { get; set; } = new XmlFileService();
        private static readonly string AppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string AppFolder = "LitWiki";
        private static readonly string RecentProjects = "RecentProjects" + FileService.GetFileExtension();
        private static readonly string ProjectsFolder = "Projects";

        public string DefaultProjectFolderPath { get; set; } = AppdataPath + "\\" + AppFolder + "\\" + ProjectsFolder;
        public string LastProjectsPath { get; set; } = AppdataPath + "\\" + AppFolder + "\\" + RecentProjects;


        public static Settings Default
        {
            get
            {
                return new Settings();
            }
        }


        private static Settings current;
        public static Settings Current
        {
            get
            {
                if (current == null)
                {
                    current = Load();
                    if (current == null)
                        current = Default;
                }

                return current;
            }
        }


        private static Settings? Load()
        {
            Settings? settings = null;

            try
            {
                if(FileService.Load("Resources/Settings.xml", out Settings? output))
                    settings = output;
            }
            catch
            {
                Save(Default);
                return Default;
            }

            return settings;
        }

        public static void Save(Settings settings)
        {
            try
            {
                FileService.Save("Resources/Settings.xml", settings);
            }
            catch
            {
                MessageBox.Show("");
            }
        }
    }
}
