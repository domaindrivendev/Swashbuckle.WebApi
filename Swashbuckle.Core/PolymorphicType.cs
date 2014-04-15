using System;
using System.Collections.Generic;

namespace Swashbuckle
{
    public class PolymorphicType
    {
        private readonly List<PolymorphicType> _subTypes;

        public PolymorphicType(Type type, bool isBase)
        {
            Type = type;
            IsBase = isBase;
            _subTypes = new List<PolymorphicType>();
        }

        internal Type Type { get; private set; }

        internal bool IsBase { get; private set; }

        internal string Discriminator { get; private set; }

        internal IEnumerable<PolymorphicType> SubTypes
        {
            get { return _subTypes; }           
        }

        internal void DiscriminateBy(string discriminator)
        {
            if (!IsBase)
                throw new InvalidOperationException("Invalid Swashbuckle configuration. Discriminator can only be applied to the base class");

            Discriminator = discriminator;
        }

        internal void RegisterSubType(PolymorphicType subTypeInfo)
        {
            if (IsBase && Discriminator == null)
                throw new InvalidOperationException("Invalid Swashbuckle configuration. Discriminator must be set on the base class ");

            _subTypes.Add(subTypeInfo);
        }

        internal PolymorphicType FindSubType(Type type)
        {
            foreach (var directSubType in _subTypes)
            {
                if (directSubType.Type == type) return directSubType;

                var nestedSubType = directSubType.FindSubType(type);
                if (nestedSubType != null) return nestedSubType;
            }

            return null;
        }
    }
}