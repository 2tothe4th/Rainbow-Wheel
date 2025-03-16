echo on
cd "C:\Users\onehu\source\repos\Rainbow Wheel\Rainbow Wheel\Output\"
ffmpeg -framerate 60 -pattern_type sequence -i frame%01d.png -s:v 1280x1280 -c:v libx264 -pix_fmt yuv420p out.mp4