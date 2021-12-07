find gen-* -type d | xargs -I {} ffmpeg -r 30 -f image2 -s 1280x1280 -i {}/frame-%d.png -vcodec libx264 -crf 25  -pix_fmt yuv420p -y {}.mp4
