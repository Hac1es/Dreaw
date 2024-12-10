using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DoAnPaint.Utils
{
    public class DrawingData
    {
        [JsonIgnore]
        public SKBitmap Bitmap {  get; set; }
        [JsonIgnore]
        public SKColor? color { get; set; }
        [JsonIgnore]
        public List<SKPoint> Points { get; set; }
        // Chuyển đổi SKColor sang uint và ngược lại để JSONify
        public uint ColorHex
        {
            get => color.HasValue
                ? (uint)((color.Value.Alpha << 24) | (color.Value.Red << 16) | (color.Value.Green << 8) | color.Value.Blue)
                : 0; // Giá trị mặc định nếu color là null
            set
            {
                byte a = (byte)((value >> 24) & 0xFF);
                byte r = (byte)((value >> 16) & 0xFF);
                byte g = (byte)((value >> 8) & 0xFF);
                byte b = (byte)(value & 0xFF);
                color = new SKColor(r, g, b, a);
            }
        }
        //Chuyển đổi SKBitmap sang chuỗi Base64 và ngược lại
        public string SyncBitmapBase64
        {
            get
            {
                if (Bitmap == null) return null;
                using (var image = Bitmap.Encode(SKEncodedImageFormat.Png, 100))
                {
                    return Convert.ToBase64String(image.ToArray());
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Bitmap = null;
                }
                else
                {
                    var imageData = Convert.FromBase64String(value);
                    Bitmap = SKBitmap.Decode(imageData);
                }
            }
        }
        //Chuyển đổi List<SKPoints> sang List<(float, float)> và ngược lại
        public List<(float X, float Y)> PointsJson
        {
            get => Points?.Select(p => (p.X, p.Y)).ToList();
            set => Points = value?.Select(p => new SKPoint(p.X, p.Y)).ToList();
        }
        public SKPoint? PointX { get; set; }
        public SKPoint? PointY { get; set; }
        public int? endX { get; set; }
        public int? endY { get; set; }
        public int? startX {  get; set; }
        public int? startY { get; set; }
        public int? width { get; set; }
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
            this.Bitmap = syncBitmap;
            this.color = coloR;
            this.Points = points;
            this.PointX = pointX;
            this.PointY = pointY;
            this.endX = endX;
            this.endY = endY;
            this.startX = startX;
            this.startY = startY;
            this.width = widtH;
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
        OCR,
        CLEAR
    }

    public enum Cursorr
    {
        NONE,
        PENCIL,
        CRAYON,
        ERASER,
        FILL
    }

    public class SyncData
    {
        [JsonIgnore]
        public SKBitmap syncBmp;
        public string currentChat {  get; set; }
        //Chuyển đổi SKBitmap sang chuỗi Base64 và ngược lại
        public string SyncBitmapBase64
        {
            get
            {
                if (syncBmp == null) return null;
                using (var image = syncBmp.Encode(SKEncodedImageFormat.Png, 100))
                {
                    return Convert.ToBase64String(image.ToArray());
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    syncBmp = null;
                }
                else
                {
                    var imageData = Convert.FromBase64String(value);
                    syncBmp = SKBitmap.Decode(imageData);
                }
            }
        }
        public SyncData(SKBitmap syncBmp, string currentChat)
        {
            this.syncBmp = syncBmp;
            this.currentChat = currentChat;
        }
    }
}
