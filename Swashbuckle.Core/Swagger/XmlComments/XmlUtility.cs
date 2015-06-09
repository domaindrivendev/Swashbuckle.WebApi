using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swashbuckle.Swagger.XmlComments
{
    public static class XmlUtility
    {
        /// <summary>
        /// Messages text from an XML node produced by Visual Studio into a plainer plain text (leading whitespace normalized)
        /// </summary>
        /// <param name="xmlText">The content of an XML node - could contain other XML elements within the string</param>
        /// <returns></returns>
        public static string NormalizeIndentation(string xmlText)
        {
            if (null == xmlText)
                throw new ArgumentNullException("xmlText");

            string[] lines = xmlText.Split('\n');
            string padding = GetCommonLeadingWhitespace(lines);

            int padLen = padding == null ? 0 : padding.Length;

            // remove leading padding from each line
            for (int i = 0, l = lines.Length; i < l; ++i)
            {
                string line = lines[i].TrimEnd('\r'); // remove trailing '\r'

                if (padLen != 0 && line.Length >= padLen && line.Substring(0, padLen) == padding)
                    line = line.Substring(padLen);

                lines[i] = line;
            }

            // remove leading empty lines, but not all leading padding
            // remove all trailing whitespace, regardless
            return string.Join("\r\n", lines.SkipWhile(x => string.IsNullOrWhiteSpace(x))).TrimEnd();
        }

        /// <summary>
        /// Finds the common padding prefix used on all non-empty lines.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns>The common padding found on all non-blank lines - returns null when no common prefix is found</returns>
        static string GetCommonLeadingWhitespace(string[] lines)
        {
            if (null == lines)
                throw new ArgumentException("lines");

            if(lines.Length == 0)
                return null;

            string[] nonEmptyLines = lines
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            if (nonEmptyLines.Length < 1)
                return null;

            int padLen = 0;

            // use the first line as a seed, and see what is shared over all nonEmptyLines
            string seed = nonEmptyLines[0];
            for (int i = 0, l = seed.Length; i < l; ++i)
            {
                if (!char.IsWhiteSpace(seed, i))
                    break;

                if (nonEmptyLines.Any(line => line[i] != seed[i]))
                    break;

                ++padLen;
            }

            if (padLen > 0)
                return seed.Substring(0, padLen);

            return null;
        }
    }
}
