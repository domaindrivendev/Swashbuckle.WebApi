using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Swashbuckle.Swagger
{
    /// <summary>
    /// Extension methods for XML objects
    /// </summary>
    public static class XmlExtensions
    {
        private static Regex ParamPattern = new Regex(@"<(see|paramref) (name|cref)=""([TPF]{1}:)?(?<display>.+?)"" />");
        private static Regex ConstPattern = new Regex(@"<c>(?<display>.+?)</c>");

        /// <summary>
        /// Extracts the display content of the specified <paramref name="node"/>, replacing
        /// paramref and c tags with a human-readable equivalent.
        /// </summary>
        /// <param name="node">The XML node from which to extract content.</param>
        /// <returns>The extracted content.</returns>
        public static string ExtractContent(this XPathNavigator node)
        {
            if (node == null) return null;

            return ConstPattern.Replace(
                ParamPattern.Replace(node.InnerXml, GetParamRefName),
                GetConstRefName).Trim();
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
    }
}
