using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Swashbuckle.Swagger.XmlComments
{
    public static class XPathNavigatorExtensiosn
    {
        private static Regex ParamPattern = new Regex(@"<(see|paramref) (name|cref)=""([TPF]{1}:)?(?<display>.+?)"" />");
        private static Regex ConstPattern = new Regex(@"<c>(?<display>.+?)</c>");
        private static Regex XmlNewLinePadding = new Regex(@"^(?<padding>[\t\v\f \u00a0\u2000-\u200b\u2028-\u2029\u3000]+)(?<force>\.?)(?<line>.*)$", RegexOptions.Multiline);
        public static string ExtractContent(this XPathNavigator node)
        {
            if (node == null) return null;

            return XmlNewLinePadding.Replace(ConstPattern.Replace(
                ParamPattern.Replace(node.InnerXml, GetParamRefName),
                GetConstRefName).Trim(), GetNewLinePadding);

        }

        private static string GetConstRefName(Match match)
        {
            if (match.Groups.Count != 2) return null;

            return match.Groups["display"].Value;
        }

        private static string GetParamRefName(Match match)
        {
            if (match.Groups.Count != 5) return null;

            return "{" + match.Groups["display"].Value + "}";
        }

        private static string GetNewLinePadding(Match match)
        {
            if (match.Groups.Count != 4) return null;

            return (match.Groups["force"].Value == "." ? " " : string.Empty) + match.Groups["line"].Value;
        }

    }
}
