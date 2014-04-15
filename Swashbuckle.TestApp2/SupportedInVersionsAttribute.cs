using System;

namespace Swashbuckle.TestApp2
{
    public class SupportedInVersionsAttribute : Attribute
    {
        public SupportedInVersionsAttribute(params string[] versions)
        {
            Versions = versions;
        }

        public string[] Versions { get; set; }
    }
}