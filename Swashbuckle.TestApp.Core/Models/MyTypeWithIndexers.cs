using System;
using System.Linq;

namespace Swashbuckle.TestApp.Models
{
    public class MyTypeWithIndexers
    {
        private readonly DateTime[] _dates = null;

        public DateTime this[int index]
        {
            get { return _dates[index]; }
        }

        public DateTime this[DayOfWeek dayOfWeek]
        {
            get { return _dates.First(date => date.DayOfWeek == dayOfWeek); }
        }
    }
}