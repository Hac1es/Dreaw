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
        public bool isPreview { get; set; }
        public SKBitmap syncBitmap {  get; set; }
        public SKColor? coloR { get; set; }
        public List<SKPoint> Points {  get; set; }
        public SKPoint? PointX { get; set; }
        public SKPoint? PointY { get; set; }
        public int? currentX {  get; set; }
        public int? currentY { get; set; }
        public int? sizeX { get; set; }
        public int? sizeY { get; set; }
        public int? startX {  get; set; }
        public int? startY { get; set; }
        public int? widtH { get; set; }
        /// <param name="ispreview">Net ve that hay gia?</param>
        /// <param name="coloR">Màu vẽ</param>
        /// <param name="widtH">Độ dày nét bút</param>
        /// <param name="pointX">điểm bắt đầu trong các tính năng nhóm Pen</param>
        /// <param name="pointY">điểm kết thúc trong các tính năng nhóm Pen</param>
        /// <param name="currentX">tọa độ X hiện tại cho các tính năng nhóm Shape</param>
        /// <param name="currentY">tọa độ Y hiện tại cho các tính năng nhóm Shape</param>
        /// <param name="sizeX">kích thước trục X cho các tính năng nhóm Shape</param>
        /// <param name="sizeY">kích thước trục Y bắt đầu cho các tính năng nhóm Shape</param>
        /// <param name="startX">tọa độ X bắt đầu cho các tính năng nhóm Shape</param>
        /// <param name="startY">tọa độ X bắt đầu cho các tính năng nhóm Shape</param>
        /// <param name="points">danh sách các điểm cho tính năng Curve và Polygon</param>
        /// <param name="syncBitmap">Bitmap dùng để đồng bộ hóa</param>
        public DrawingData(bool ispreview, SKColor? coloR = null, int? widtH = null, SKPoint? pointX = null, SKPoint? pointY = null, int? currentX = null, int? currentY = null, int? sizeX = null, int? sizeY = null, int? startX = null, int? startY = null, List<SKPoint> points = null, SKBitmap syncBitmap = null)
        {
            this.isPreview = ispreview;
            this.syncBitmap = syncBitmap;
            this.coloR = coloR;
            Points = points;
            PointX = pointX;
            PointY = pointY;
            this.currentX = currentX;
            this.currentY = currentY;
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.startX = startX;
            this.startY = startY;
            this.widtH = widtH;
            this.isPreview = isPreview;
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
