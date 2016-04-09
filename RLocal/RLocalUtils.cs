using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace DesktopDup
{
    class RLocalUtils
    {
        public static int GetSizeBGRA(int width, int height)
        {
            int stride = 4 * width;
            return stride * height;
        }
    }
}
