using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Utilities
{
    public interface IDateTimeManager
    { 
        DateTime Today { get; }
    }

    public class DateTimeManager : IDateTimeManager
    {
        public DateTimeManager()
        {
            Today = DateTime.UtcNow;
        }
        public DateTime Today { get; }
    }
}
