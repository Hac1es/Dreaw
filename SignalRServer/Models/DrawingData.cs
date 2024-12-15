using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public static class General
    {
        public static string SQLServer = @"Server=TUNGFTUNGFTUNGF;Database=DreawDB;Trusted_Connection=True;";
    }
    public enum Command
    {
        CURSOR,
        SYNC,
        PENCIL,
        CRAYON,
        ERASER,
        FILL,
        LINE,
        RECTANGLE,
        ELLIPSE,
        CURVE,
        POLYGON,
        OCR
    }
}
