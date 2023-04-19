using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;
using LitWiki.Tools;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace LitWiki.Models
{
    [Serializable]
    [DataContract]
    public class ProjectMetadata
    {
        [DataMember]
        public string Name { get; set; } = string.Empty;

        private string folderPath = string.Empty;
        [DataMember]
        public string FolderPath
        {
            get { return folderPath; }
            set
            {
                folderPath = value.Replace('\\', '/');
            }
        }
        [DataMember]
        public DateTime CreationDate { get; set; }
        [DataMember]
        public DateTime LastEditedDate { get; set; }



        public ProjectMetadata()
        {

        }



        public string GetFullPath(IFileService fileService)
        {
            return FolderPath + "/" + Name + fileService.GetFileExtension();
        }

        public void SetFullPath(string path)
        {
            int lastSlashIndex = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
            int lastPointIndex = path.LastIndexOf('.');

            FolderPath = path.Substring(0, lastSlashIndex);
            Name = path.Substring(lastSlashIndex + 1, lastPointIndex - lastSlashIndex - 1);
        }
    }
}//         111111111
// 123456789012345678
// test/test/test.xml