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
        CURSOR,
        START,
        END,
        SYNC,
        PENCIL,
        CRAYON,
        ERASER,
        FILL,
        LINE,
        RECTANGLE,
        ELLIPSE,
        BEIZER,
        POLYGON,
        OCR
    }

    public enum Cursorr
    {
        NONE,
        PENCIL,
        CRAYON,
        ERASER,
        FILL
    }
}
