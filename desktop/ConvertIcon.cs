using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class Program
{
    static void Main()
    {
        string pngPath = @"frontend\src\images\image.png";
        string icoPath = @"desktop\logo.ico";

        using (Bitmap bmp = new Bitmap(pngPath))
        using (Bitmap resized = new Bitmap(bmp, new Size(64, 64)))
        using (MemoryStream ms = new MemoryStream())
        using (MemoryStream pngMs = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            resized.Save(pngMs, ImageFormat.Png);
            byte[] pngBytes = pngMs.ToArray();

            bw.Write((short)0);   // Reserved
            bw.Write((short)1);   // Type 1 = ICO
            bw.Write((short)1);   // Count 1 image
            bw.Write((byte)64);   // Width
            bw.Write((byte)64);   // Height
            bw.Write((byte)0);    // Colors
            bw.Write((byte)0);    // Reserved
            bw.Write((short)1);   // Planes
            bw.Write((short)32);  // BPP
            bw.Write((int)pngBytes.Length); // Size
            bw.Write((int)22);    // Offset

            bw.Write(pngBytes);

            File.WriteAllBytes(icoPath, ms.ToArray());
            Console.WriteLine("Successfully created valid Win32 desktop/logo.ico!");
        }
    }
}
