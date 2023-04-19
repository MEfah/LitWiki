using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitWiki.Tools
{
    public interface IFileService
    {
        public void Save<T>(string path, T toSave);
        public bool Load<T>(string path, out T? result);
        public string GetFileExtension();
        public string GetFileExtensionsDescription();
    }
}
