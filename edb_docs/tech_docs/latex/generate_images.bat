REM Install ImageMagick https://imagemagick.org
forfiles /m *.pdf /s /c "cmd /c convert -density 300 @file -quality 90 -background white -flatten @fname.png"