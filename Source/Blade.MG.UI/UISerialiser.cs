using System.Data;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Blade.MG.UI
{
    public static class UISerialiser
    {
        //public static string ToJson<T>(T uiwindow) where T : UIWindow
        //{
        //    return JsonConvert.SerializeObject(uiwindow, Newtonsoft.Json.Formatting.Indented);
        //}

        //public static UIWindow FromJson(string uistring)
        //{
        //    var window = JsonConvert.DeserializeObject<UIWindow>(uistring);
        //    return window;
        //}

        //public static string ToXml<T>(T uiwindow) where T : UIWindow
        //{
        //    using (var stringwriter = new System.IO.StringWriter())
        //    {
        //        var serializer = new XmlSerializer(typeof(T));
        //        serializer.Serialize(stringwriter, uiwindow);
        //        return stringwriter.ToString();
        //    }
        //}

        //public static T FromXml<T>(string xmlText) where T : UIWindow
        //{
        //    using (var stringReader = new System.IO.StringReader(xmlText))
        //    {
        //        var serializer = new XmlSerializer(typeof(T));
        //        return serializer.Deserialize(stringReader) as T;
        //    }
        //}

        public static string Serialize(object obj, List<Type> knownTypes = null)
        {
            bool prettyPrintXml = true;

            knownTypes = knownTypes ?? new List<Type>();

            //---------------------------------------------------------------
            /*
            XmlSerializer ser = new XmlSerializer(obj.GetType());

            using (MemoryStream memoryStream = new MemoryStream())
            using (TextWriter writer = new StreamWriter(memoryStream))
            {
                ser.Serialize(writer, obj);
                writer.Close();

                memoryStream.Position = 0;


                using (StreamReader reader = new StreamReader(memoryStream))
                {
                    return reader.ReadToEnd();
                }

            }
            */

            //---------------------------------------------------------------

            knownTypes.AddRange(GetKnownTypes(Assembly.GetCallingAssembly()));

            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType(), knownTypes);
                var settings = new XmlWriterSettings { Indent = true, };

                if (prettyPrintXml)
                {
                    // Pretty-Print XML
                    using (XmlWriter writer = XmlWriter.Create(memoryStream, settings))
                    {
                        //serializer.WriteObject(writer, obj);

                        serializer.WriteStartObject(writer, obj);

                        //writer.WriteAttributeString("xmlns", "x", null, "http://www.w3.org/2001/XMLSchema");
                        writer.WriteAttributeString("xmlns", "ui", null, "http://schemas.datacontract.org/2004/07/Blade.Games.UI");
                        writer.WriteAttributeString("xmlns", "uic", null, "http://schemas.datacontract.org/2004/07/Blade.Games.UI.Components");

                        serializer.WriteObjectContent(writer, obj);
                        serializer.WriteEndObject(writer);
                    }
                }
                else
                {
                    serializer.WriteObject(memoryStream, obj);
                }

                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }

        }

        public static T Deserialize<T>(string rawXml)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(rawXml)))
            {
                DataContractSerializer formatter0 = new DataContractSerializer(typeof(T));
                return (T)formatter0.ReadObject(reader);
            }
        }


        private static IEnumerable<Type> GetKnownTypes(Assembly assembly)
        {
            IEnumerable<Type> knownTypes = null;

            knownTypes = assembly.GetTypes()
                                 .Where(t => typeof(UIComponent).IsAssignableFrom(t))
                                 .ToList();
            return knownTypes;
        }

    }
}
