using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAnPaint.Utils
{
    public class DrawingData
    {
        public SKBitmap syncBitmap {  get; set; }
        public SKColor? coloR { get; set; }
        public List<SKPoint> Points {  get; set; }
        public SKPoint? PointX { get; set; }
        public SKPoint? PointY { get; set; }
        public int? endX { get; set; }
        public int? endY { get; set; }
        public int? startX {  get; set; }
        public int? startY { get; set; }
        public int? widtH { get; set; }
        /// <param name="coloR">Màu vẽ</param>
        /// <param name="widtH">Độ dày nét bút</param>
        /// <param name="pointX">điểm bắt đầu trong các tính năng nhóm Pen</param>
        /// <param name="pointY">điểm kết thúc trong các tính năng nhóm Pen</param>
        /// <param name="endX">kích thước trục X cho các tính năng nhóm Shape</param>
        /// <param name="endY">kích thước trục Y bắt đầu cho các tính năng nhóm Shape</param>
        /// <param name="startX">tọa độ X bắt đầu cho các tính năng nhóm Shape</param>
        /// <param name="startY">tọa độ X bắt đầu cho các tính năng nhóm Shape</param>
        /// <param name="points">danh sách các điểm cho tính năng Curve và Polygon</param>
        /// <param name="syncBitmap">Bitmap dùng để đồng bộ hóa</param>
        public DrawingData(SKColor? coloR = null, int? widtH = null, SKPoint? pointX = null, SKPoint? pointY = null, int? startX = null, int? startY = null, int? endX = null, int? endY = null, List<SKPoint> points = null, SKBitmap syncBitmap = null)
        {
            this.syncBitmap = syncBitmap;
            this.coloR = coloR;
            this.Points = points;
            this.PointX = pointX;
            this.PointY = pointY;
            this.endX = endX;
            this.endY = endY;
            this.startX = startX;
            this.startY = startY;
            this.widtH = widtH;
        }
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

    public enum Cursorr
    {
        NONE,
        PENCIL,
        CRAYON,
        ERASER,
        FILL
    }
}
