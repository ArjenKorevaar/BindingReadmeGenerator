namespace BindingReadmeGenerator
{
    public static class XmlSerializer
    {
        public static T Deserialize<T>(string filename)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T), "http://eclipse.org/smarthome/schemas/thing-description/v1.0.0");

            var filestream = new System.IO.FileStream(filename, System.IO.FileMode.Open);
            var reader = new System.Xml.XmlTextReader(filestream);

            return (T)serializer.Deserialize(reader );
        }

        public static string Serialize(object obj)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            var ns = new System.Xml.Serialization.XmlSerializerNamespaces();
            ns.Add("thing", "http://eclipse.org/smarthome/schemas/thing-description/v1.0.0");

            using (var writer = new System.IO.StringWriter())
            {
                serializer.Serialize(writer, obj, ns);
                return writer.ToString();
            }
        }
    }
}
