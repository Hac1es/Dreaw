using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
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
        ERASER,
        FILL,
        LINE,
        RECTANGLE,
        ELLIPSE,
        BEIZER,
        POLYGON
    }

    public enum Cursorr
    {
        NONE,
        PENCIL,
        ERASER,
        FILL
    }
}
