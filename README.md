# FFMPEG video test diffusion on : rtmp://localhost:1935/live1/test
```bash
ffmpeg -re -f lavfi -i testsrc=size=1280x720:rate=30 -f lavfi -i sine=frequency=1000 -c:v libx264 -preset veryfast -pix_fmt yuv420p -c:a aac -ar 44100 -ac 2 -f flv rtmp://localhost:1935/live1/test
```
