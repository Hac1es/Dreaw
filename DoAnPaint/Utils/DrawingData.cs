using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAnPaint.Utils
{
    public class DrawingData
    {
    }

    public enum Command
    {
        START,
        END,
        SYNC,
        PENCIL,
        ERASER,
        FILL,
        LINE,
        RECTANGLE
    }
}
