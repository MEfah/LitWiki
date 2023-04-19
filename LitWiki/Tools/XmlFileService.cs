using LitWiki.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace LitWiki.Tools
{
    public class XmlFileService : IFileService
    {
        public XmlFileService() 
        {

        }

        public bool Load<T>(string path, out T? result)
        {
            result = default(T);
            if (!typeof(T).IsSerializable)
                throw new Exception("XmlFileService can only operate on serializable objects");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
            {
                object? deserialized = xmlSerializer.Deserialize(fs);

                if (deserialized is T)
                {
                    result = (T)deserialized;
                    return true;
                }
                else return false;
            }
        }

        public void Save<T>(string path, T toSave)
        {
            if (!typeof(T).IsSerializable)
                throw new InvalidOperationException("XmlFileService can only operate on serializable objects");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                xmlSerializer.Serialize(fs, toSave);
            }
        }

        public string GetFileExtension()
        {
            return ".xml";
        }

        public string GetFileExtensionsDescription()
        {
            return "Xml файлы|*.xml";
        }
    }
}
