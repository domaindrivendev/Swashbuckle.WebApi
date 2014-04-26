using System;
using System.Collections.Generic;

namespace Swashbuckle.Dummy.Models
{
    public class Unicorn
    {
        public Magic<string> MagicName { get; set; }

        public Spells Spells { get; set; }
    }

    public class Magic<T>
    {
        public T Normal { get; set; }
    }

    public class Spells
    {
        public List<Spell> All { get; set; }

        public Spell this[int rank]
        {
            get { throw new NotImplementedException(); }
        }

        public Spell this[string name]
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class Spell
    {
        public string Name { get; set; }
    }
}