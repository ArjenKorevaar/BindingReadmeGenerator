using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BindingReadmeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"D:\Repositories\Github\openhab2-addons\addons\binding\org.openhab.binding.openthermgateway";

            string configPath = Path.Combine(path, "ESH-INF", "config");
            string thingPath = Path.Combine(path, "ESH-INF", "thing");

            var configDescriptions = GetConfigDescriptions(configPath);
            var thingDescriptions = GetThingDescriptions(thingPath);

            var thingTypes = thingDescriptions.Items.Where(p => p is thingType).Select(p => p as thingType);
            var channelTypes = thingDescriptions.Items.Where(p => p is channelType).Select(p => p as channelType);

            StringBuilder itemsBuilder = new StringBuilder();
            StringBuilder sitemapBuilder = new StringBuilder();

            sitemapBuilder.AppendLine(@"sitemap demo label=""Main Menu"" {");

            string thingName = string.Empty;

            foreach (var thing in thingTypes)
            {
                thingName = thing.label;

                sitemapBuilder.AppendLine(string.Format(@"    Frame label=""{0}"" {{", thingName));

                foreach (var channel in (thing.Item as channels).channel)
                {
                    var channelType = channelTypes.FirstOrDefault(p => p.id == channel.typeId);

                    itemsBuilder.AppendLine(CreateItemsLine(channel, channelType));
                    sitemapBuilder.AppendLine(CreateSitemapsLine(channel, channelType));
                }

                sitemapBuilder.AppendLine(@"    }");
            }

            sitemapBuilder.AppendLine(@"}");
            
            StringBuilder channelsBuilder = new StringBuilder();

            foreach (var channelType in channelTypes)
            {
                channelsBuilder.AppendLine("- " + channelType.label);
            }
            
            string readmePath = Path.Combine(path, "readme.md");
            string readme = File.ReadAllText(readmePath);

            readme = readme.Substring(0, readme.IndexOf("## Channels"));

            StringBuilder readmeBuilder = new StringBuilder(readme);

            readmeBuilder.AppendHeader("Channels", 2);
            readmeBuilder.AppendLine(string.Format("The {0} supports the following channels:", thingName));
            readmeBuilder.AppendLine(channelsBuilder.ToString());

            readmeBuilder.AppendHeader("Full Example", 2);

            readmeBuilder.AppendHeader("demo.things", 3);
            readmeBuilder.AppendCode(@"Thing openthermgateway:otgw:1 [ipaddress=""192.168.1.100"", port=""8000""]");

            readmeBuilder.AppendHeader("demo.items", 3);
            readmeBuilder.AppendCode(itemsBuilder.ToString());

            readmeBuilder.AppendHeader("demo.sitemap", 3);
            readmeBuilder.AppendCode(sitemapBuilder.ToString());

            File.WriteAllText(readmePath, readmeBuilder.ToString());
        }

        private static string CreateItemsLine(channel channel, channelType channelType)
        {
            string icon = string.Empty;

            if (channelType.itemtype == "Switch")
            {
                icon = " <switch>";
            }
            else if (channelType.category == "temperature")
            {
                icon = " <temperature>";
            }

            return string.Format(@"{0} {1} ""{2}""{3} {{channel=""openthermgateway:otgw:1:{4}}}", channelType.itemtype, channelType.label.ToPascalCase(), CreateLabel(channelType), icon, channel.id);
        }

        private static string CreateSitemapsLine(channel channel, channelType channelType)
        {
            string itemType = "Text";
            string additionalValues = string.Empty;

            if (channelType.itemtype == "Switch")
            {
                itemType = "Switch";
            }
            else if (!channelType.state.readOnly && channelType.category == "temperature")
            {
                itemType = "Setpoint";
            }

            if (!channelType.state.readOnly)
            {
                if (channelType.state.minSpecified)
                {
                    additionalValues += string.Format(@" minValue=""{0}""", channelType.state.min.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                if (channelType.state.maxSpecified)
                {
                    additionalValues += string.Format(@" maxValue=""{0}""", channelType.state.max.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                if (channelType.state.stepSpecified)
                {
                    additionalValues += string.Format(@" step=""{0}""", channelType.state.step.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }            

            string icon = string.Empty;

            if (channelType.itemtype == "Switch")
            {
                icon = "switch";
            }
            else if (channelType.category == "temperature")
            {
                icon = "temperature";
            }

            return string.Format(@"        {0} item=""{1}"" icon=""{2}"" label=""{3}""{4}", itemType, channelType.label.ToPascalCase(), icon, CreateLabel(channelType), additionalValues);
        }

        private static string CreateLabel(channelType channelType)
        {
            return string.IsNullOrEmpty(channelType.state.pattern) ? channelType.label : channelType.label + " [" + channelType.state.pattern + "]";
        }

        private static configdescriptions GetConfigDescriptions(string path)
        {
            List<configDescription> result = new List<configDescription>();

            foreach (var xmlfile in GetXmlFiles(path))
            {
                var config = XmlSerializer.Deserialize<configdescriptions>(xmlfile.FullName);
                result.AddRange(config.configdescription);
            }

            return new configdescriptions { configdescription = result.ToArray() };
        }

        private static thingdescriptions GetThingDescriptions(string path)
        {
            List<object> result = new List<object>();

            string bindingId = string.Empty;

            foreach (var xmlfile in GetXmlFiles(path))
            {
                var things = XmlSerializer.Deserialize<thingdescriptions>(xmlfile.FullName);
                result.AddRange(things.Items);

                bindingId = things.bindingId;
            }

            return new thingdescriptions { bindingId = bindingId, Items = result.ToArray() };
        }

        private static FileInfo[] GetXmlFiles(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            return directoryInfo.GetFiles("*.xml");
        }
    }
}
