using System;

namespace Swashbuckle.Swagger
{
    public class AreaDescription
    {
        public string Name { get; }
        public Type RegistrationType { get; }

        public AreaDescription(string name, Type registrationType)
        {
            Name = name;
            RegistrationType = registrationType;
        }

        public static AreaDescription Empty = new AreaDescription(string.Empty, null);

        public static bool IsNullOrEmpty(AreaDescription area)
        {
            return area == null || area.Equals(Empty);
        }


        #region Equality members

        protected bool Equals(AreaDescription other)
        {
            return string.Equals(Name, other.Name) && Equals(RegistrationType, other.RegistrationType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AreaDescription) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (RegistrationType != null ? RegistrationType.GetHashCode() : 0);
            }
        }

        #endregion
    }
}
