using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BindingReadmeGenerator
{
    public static class Extensions
    {
        public static string ToPascalCase(this string value)
        {
            var chars = value.ToCharArray();

            string result = string.Empty;

            bool toUpper = true;

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i].ToString() == " ")
                {
                    toUpper = true;
                }
                else
                {
                    if (toUpper)
                    {
                        result += chars[i].ToString().ToUpper();
                        toUpper = false;
                    }
                    else
                    {
                        result += chars[i].ToString();
                    }
                }
            }

            return result;
        }

        public static void AppendCode(this StringBuilder builder, string content)
        {
            builder.AppendLine("```");
            builder.AppendLine(content);
            builder.AppendLine("```");
        }

        public static void AppendHeader(this StringBuilder builder, string header, int level)
        {
            builder.AppendLine();
            var indent = new string('#', level);
            builder.AppendLine(string.Format("{0} {1}", indent, header));
        }
    }
}
