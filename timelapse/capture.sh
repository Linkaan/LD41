ffmpeg -video_size 1920x1080 -f x11grab -i :0.0+0,0 -f image2 -vf fps=1/15 images/$(date +"%Y%m%d_%H%M%S")_%03d.png
