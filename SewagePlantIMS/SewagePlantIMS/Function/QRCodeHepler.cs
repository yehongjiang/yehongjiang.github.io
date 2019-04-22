using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace SewagePlantIMS.Function
{

    public class QRCodeHelper

    {

        /// <summary>  
        /// 生成二维码  
        /// </summary>  
        /// <param name="content">内容</param>
        /// <param name="moduleSize">二维码的大小</param>
        /// <returns>输出流</returns>  
        public static MemoryStream GetQRCode(string content, int moduleSize)
        {
            //ErrorCorrectionLevel 误差校正水平
            //QuietZoneModules     空白区域
            var encoder = new QrEncoder(ErrorCorrectionLevel.M);
            QrCode qrCode = encoder.Encode(content);
            GraphicsRenderer render = new GraphicsRenderer(new FixedModuleSize(moduleSize, QuietZoneModules.Two), Brushes.Black, Brushes.White);
            MemoryStream memoryStream = new MemoryStream();
            render.WriteToStream(qrCode.Matrix, ImageFormat.Jpeg, memoryStream);
            return memoryStream;
            //生成图片的代码
            //DrawingSize dSize = render.SizeCalculator.GetSize(qrCode.Matrix.Width);
            //Bitmap map = new Bitmap(dSize.CodeWidth, dSize.CodeWidth);
            //Graphics g = Graphics.FromImage(map);
            //render.Draw(g, qrCode.Matrix);
            //map.Save(fileName, ImageFormat.Jpeg);//fileName为存放的图片路径
        }
        /// <summary>
        /// 生成带Logo二维码  
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="iconPath">logo路径</param>
        /// <param name="moduleSize">二维码的大小</param>
        /// <returns>输出流</returns>
        public static MemoryStream GetQRCode(string content, string iconPath, int moduleSize = 9)
        {
            QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.M);
            QrCode qrCode = qrEncoder.Encode(content);
            GraphicsRenderer render = new GraphicsRenderer(new FixedModuleSize(moduleSize, QuietZoneModules.Two), Brushes.Black, Brushes.White);
            DrawingSize dSize = render.SizeCalculator.GetSize(qrCode.Matrix.Width);
            Bitmap map = new Bitmap(dSize.CodeWidth, dSize.CodeWidth);
            Graphics g = Graphics.FromImage(map);
            render.Draw(g, qrCode.Matrix);
            //追加Logo图片 ,注意控制Logo图片大小和二维码大小的比例
            //PS:追加的图片过大超过二维码的容错率会导致信息丢失,无法被识别
            Image img = Image.FromFile(iconPath);
            Point imgPoint = new Point((map.Width - img.Width) / 2, (map.Height - img.Height) / 2);
            g.DrawImage(img, imgPoint.X, imgPoint.Y, img.Width, img.Height);
            MemoryStream memoryStream = new MemoryStream();
            map.Save(memoryStream, ImageFormat.Jpeg);
            string fileName = "~/image/";
            map.Save(fileName, ImageFormat.Jpeg);
            return memoryStream;
            //生成图片的代码： map.Save(fileName, ImageFormat.Jpeg);//fileName为存放的图片路径
        }


        public static Image AddTextToImg(Image preimg, string text, string picname)
        {
            //判断指定图片是否存在
            /* if (!File.Exists(Server.MapPath(fileName)))
             {
                 throw new FileNotFoundException("The file don't exist!");
             }*/
            Bitmap bitmap = new Bitmap(preimg, preimg.Width, preimg.Height);
            Graphics g = Graphics.FromImage(bitmap);
            float fontSize = 10.0f;             //字体大小
            float textWidth = text.Length * fontSize;  //文本的长度
                                                       //下面定义一个矩形区域，以后在这个矩形里画上白底黑字
            float rectY = 350;
            float rectX = 15;
            float rectWidth = text.Length * (fontSize + 40);
            float rectHeight = fontSize + 40;
            //声明矩形域
            RectangleF textArea = new RectangleF(rectX, rectY, rectWidth, rectHeight);
            Font font = new Font("微软雅黑", fontSize, FontStyle.Bold);   //定义字体
                                                                      //font.Bold = true;
            Brush whiteBrush = new SolidBrush(Color.Black);   //白笔刷，画文字用
                                                              //Brush blackBrush = new SolidBrush(Color.Black);   //黑笔刷，画背景用
                                                              //g.FillRectangle(blackBrush, rectX, rectY, rectWidth, rectHeight);
            g.DrawString(text, font, whiteBrush, textArea);
            Image img = bitmap;
            g.Dispose();
            preimg.Dispose();
            return img;
            //bitmap.Save(path, ImageFormat.Png);
            //输出方法二，显示在网页中，保存为Jpg类型
            //bitmap.Save(ms, ImageFormat.Jpeg);
            //Response.Clear();
            //Response.ContentType = "image/jpeg";
            //Response.BinaryWrite(ms.ToArray());
        }
    }

  

}