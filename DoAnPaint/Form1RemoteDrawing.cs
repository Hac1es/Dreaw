using DoAnPaint.Utils;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAnPaint
{
    public partial class Form1
    {
        ConcurrentQueue<(DrawingData, Command)> QueueBOT = new ConcurrentQueue<(DrawingData, Command)>();
        SKPaint remote_pen = new SKPaint { IsAntialias = true };
        SKPaint remote_penenter = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };
        SKPaint remote_crayon = new SKPaint { IsAntialias = true };
        SKImageInfo xuc_tac;
        SKSurface xuc_tac_2;
        #region Supporters
        private void SetPen(ref SKPaint pen, DrawingData data)
        {
            pen.Color = (SKColor)data.coloR;
            pen.StrokeWidth = (int)data.widtH;
        }
        private void SetCrayon(ref SKPaint pen, DrawingData data)
        {
            pen.Shader = CrayonTexture((SKColor)data.coloR, (int)data.widtH * 4);
        }
        private void SetEraser(ref SKPaint pen, DrawingData data)
        {
            pen.Color = SKColors.White;
            pen.StrokeWidth = (int)data.widtH * 4;
        }
        #endregion
        private Task HandleData(DrawingData data, Command flag)
        {
            SKCanvas remote_canvas = xuc_tac_2.Canvas;
            switch(flag) 
            {
                case Command.PENCIL:
                    SetPen(ref remote_pen, data);
                    remote_canvas.DrawLine((SKPoint)data.PointX, (SKPoint)data.PointY, remote_pen);
                    break;
                case Command.CRAYON:
                    SetCrayon(ref remote_crayon, data);
                    remote_canvas.DrawLine((SKPoint)data.PointX, (SKPoint)data.PointY, remote_pen);
                    break;
                case Command.ERASER:
                    SetEraser(ref remote_pen, data);
                    remote_canvas.DrawLine((SKPoint)data.PointX, (SKPoint)data.PointY, remote_pen);
                    break;
            }
                          
        }
    }
}
