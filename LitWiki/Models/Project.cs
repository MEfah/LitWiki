using LitWiki.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LitWiki.Models
{
    [DataContract]
    public class Project
    {
        // TODO: Maybe not save Metadata (i.e. remove DataMember attribute)
        // In this case probably will have to get metadata from file itself

        [DataMember]
        public ProjectMetadata Metadata { get; set; }

        [DataMember]
        public List<Directory> Directories { get; set; }

        public Project()
        {
            Directories = new List<Directory>();
        }



        public static Project? Load(ProjectMetadata projectMetadata, IFileService fileService)
        {
            if (fileService.Load(projectMetadata.GetFullPath(fileService), out Project? p))
            {
                if(p != null)
                {
                    p.Metadata.CreationDate = File.GetCreationTime(projectMetadata.GetFullPath(fileService));
                    p.Metadata.LastEditedDate = File.GetLastWriteTime(projectMetadata.GetFullPath(fileService));
                }

                return p;
            }
            return null;
        }

        public static Project? CreateEmptyProject(ProjectMetadata projectMetadata, IFileService fileService)
        {
            Project? project = new Project() { Metadata = projectMetadata};
            project.Directories.AddRange(new List<Directory>()
            {
                new Directory() { Name = "Персонажи" },
                new Directory() { Name = "Места" },
                new Directory() { Name="События" }
            });
            try
            {
                fileService.Save(projectMetadata.GetFullPath(fileService), project);
                projectMetadata.CreationDate = DateTime.Now;
                projectMetadata.LastEditedDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                project = null;
                Debug.WriteLine("При создании пустого проекта возникла ошибка: \n" + ex);
            }
            return project;
        }
    }
}
