using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmAndTimer
{    public record InputItem(string Type, string AmPm, string Hour, string Minute, string Second, string? Memo);
}
