using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LitWiki.Tools
{
    class DataContractFileService : IFileService
    {
        public string GetFileExtension()
        {
            return ".litmx";
        }

        public string GetFileExtensionsDescription()
        {
            return "LitWiki модель|*.litmx|Xml файлы|*.xml";
        }

        public bool Load<T>(string path, out T? result)
        {
            result = default(T);

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas() { MaxDepth=200 }))
                    {
                        DataContractSerializer ser = new DataContractSerializer(typeof(T));

                        result = (T?)ser.ReadObject(reader, true);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public void Save<T>(string path, T toSave)
        {
            FileStream writer = new FileStream(path, FileMode.Create);
            DataContractSerializer ser = new DataContractSerializer(typeof(T));
            ser.WriteObject(writer, toSave);
            writer.Close();
        }
    }
}
