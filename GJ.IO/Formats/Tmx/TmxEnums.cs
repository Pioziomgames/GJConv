using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TmxLib
{
    public class TmxEnums
    {
        public enum TmxPixelFormat
        {
            PSMTC32 = 0x00,
            PSMTC24 = 0x01,
            PSMTC16 = 0x02,
            PSMTC16S = 0x0A,
            PSMT8 = 0x13,
            PSMT4 = 0x14,
            PSMT8H = 0x1B,
            PSMT4HL = 0x24,
            PSMT4HH = 0x2C,
        }
        public enum TmxWrapMode
        {
            URepeatVRepeat = 0x00,
            UClampVRepeat  = 0x01,
            URepeatVClamp  = 0x04,
            UClampVClamp   = 0x05,
            Off            = 0xFF,
        }
    }
}
