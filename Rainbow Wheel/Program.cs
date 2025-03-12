using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using FFMpegCore;
using FFMpegCore.Enums;

namespace RainbowWheel
{

    internal class Program
    {
        //https://stackoverflow.com/questions/1335426/is-there-a-built-in-c-net-system-api-for-hsv-to-rgb
        public static Color HsvToRgb(double h, double S, double V)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            return Color.FromArgb(Clamp((int)(R * 255.0)), Clamp((int)(G * 255.0)), Clamp((int)(B * 255.0)));
        }
        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        public static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
        //https://stackoverflow.com/questions/1531695/round-to-nearest-five
        public static double FloorToNearest(double input, double interval)
        { 
            return interval * Math.Floor(input / interval);
        }
        static void Main(string[] args)
        {
            //https://www.shutterstock.com/blog/common-aspect-ratios-photo-image-sizes#:~:text=A%201%3A1%20ratio%20means,template%20on%20social%20media%20sites.
            string parentDirectory = Console.ReadLine(); 
            
            double frameRate = 60;
            double frequency = 0.25;
            double screenWidth = 720;
            double screenHeight = 720;

            //Haskell naming scheme
            for (int f = 0; f < frameRate / frequency; f++)
            {
                Bitmap image = new Bitmap((int)screenWidth, (int)screenHeight);

                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        Complex position = new Complex(x, (screenHeight - 1) - y) / (screenWidth - 1) * 2 - new Complex(1, 1);
                        //https://learn.microsoft.com/en-us/dotnet/api/microsoft.toolkit.uwp.helpers.colorhelper.fromhsv?view=win-comm-toolkit-dotnet-7.1
                        //https://stackoverflow.com/questions/1335426/is-there-a-built-in-c-net-system-api-for-hsv-to-rgb
                        //https://stackoverflow.com/questions/1531695/round-to-nearest-five
                        double currentAngle = FloorToNearest(position.Phase - f / frameRate * frequency * double.Tau, double.Pi / 8);
                        image.SetPixel(x, y, HsvToRgb(currentAngle / double.Tau * 360, 1, 1));
                    }
                }
                image.Save(Path.Join(parentDirectory, $"frame{f}.png"));
                Console.WriteLine($"Frame {f} finished");
            }
            /*
            Some links for FFMPEG:  
            https://stackoverflow.com/questions/24961127/how-to-create-a-video-from-images-with-ffmpeg  
            https://askubuntu.com/questions/648603/how-to-create-an-animated-gif-from-mp4-video-via-command-line/837574#837574  
            https://www.reddit.com/r/VideoEditing/comments/yxnco2/is_there_a_way_to_convert_an_image_sequence_into/
            
            Shell:  
            https://en.wikipedia.org/wiki/Shell_script  
            https://en.wikipedia.org/wiki/Bash_(Unix_shell) 

            //http://convertio.co/mp4-gif/
            */
            Process.Start(
                $"cd {parentDirectory}\n" +
                "ffmpeg -framerate 60 -pattern_type sequence -i frame%01d.png -s:v 1920x1080 -c:v libx264 -pix_fmt yuv420p out.mp4");
        }
    }
}