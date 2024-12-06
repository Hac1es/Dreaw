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
        public bool Painting { get; set; }
        public List<SKPoint> Points {  get; set; }
        public List<SKPoint> TempPoints { get; set; }
        public SKPoint? PointX { get; set; }
        public SKPoint? PointY { get; set; }
        public int? currentX {  get; set; }
        public int? currentY { get; set; }
        public int? sizeX { get; set; }
        public int? sizeY { get; set; }
        public int? startX {  get; set; }
        public int? startY { get; set; }
        public int? widtH { get; set; }
        /// <param name="coloR">Màu vẽ(nullable)</param>
        /// <param name="witdH">Độ dày nét bút(nullable)</param>
        /// <param name="painting">Có đang vẽ không?(bắt buộc)</param>
        /// <param name="pointX">điểm bắt đầu trong các tính năng nhóm Pen(nullable)</param>
        /// <param name="pointY">điểm kết thúc trong các tính năng nhóm Pen(nullable)</param>
        /// <param name="currentX">tọa độ X hiện tại cho các tính năng nhóm Shape(nullable)</param>
        /// <param name="currentY">tọa độ Y hiện tại cho các tính năng nhóm Shape(nullable)</param>
        /// <param name="sizeX">kích thước trục X cho các tính năng nhóm Shape(nullable)</param>
        /// <param name="sizeY">kích thước trục Y bắt đầu cho các tính năng nhóm Shape(nullable)</param>
        /// <param name="startX">tọa độ X bắt đầu cho các tính năng nhóm Shape(nullable)</param>
        /// <param name="startY">tọa độ X bắt đầu cho các tính năng nhóm Shape(nullable)</param>
        /// <param name="points">danh sách các điểm cho tính năng Curve và Polygon(không bắt buộc)</param>
        /// <param name="tempPoints">danh sách tạm các điểm cho tính năng Curve và Polygon(không bắt buộc)</param>
        /// <param name="syncBitmap">Bitmap dùng để đồng bộ hóa</param>
        public DrawingData(SKColor? coloR, int? widtH, bool painting, SKPoint? pointX, SKPoint? pointY, int? currentX, int? currentY, int? sizeX, int? sizeY, int? startX, int? startY, List<SKPoint> points = null, List<SKPoint> tempPoints = null, SKBitmap syncBitmap = null)
        {
            this.syncBitmap = syncBitmap;
            this.coloR = coloR;
            Painting = painting;
            Points = points;
            TempPoints = tempPoints;
            PointX = pointX;
            PointY = pointY;
            this.currentX = currentX;
            this.currentY = currentY;
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.startX = startX;
            this.startY = startY;
            this.widtH = widtH;
        }
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
