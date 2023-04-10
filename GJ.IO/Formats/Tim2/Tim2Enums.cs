using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tim2Lib
{
    public class Tim2Enums
    {
        public enum Tim2Alignment
        {
            NoAlign = 0,
            Align16 = 1,
            Align128 = 2,
        }
        public enum Tim2BPP
        {
            RGBA5551 = 1,
            RGBA8880 = 2,
            RGBA8888 = 3,
            INDEX4   = 4,
            INDEX8   = 5,
        }
    }
}
