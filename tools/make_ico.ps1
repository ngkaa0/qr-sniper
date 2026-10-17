Add-Type -AssemblyName System.Drawing

if (Test-Path "icon.png") {
    try {
        $src = [System.Drawing.Image]::FromFile("icon.png")
        $sizes = @(16, 20, 24, 32, 48, 64, 128, 256)
        $streams = @()

        foreach ($sz in $sizes) {
            $bmp = New-Object System.Drawing.Bitmap $sz, $sz
            $g = [System.Drawing.Graphics]::FromImage($bmp)
            $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $g.Clear([System.Drawing.Color]::Transparent)
            $g.DrawImage($src, 0, 0, $sz, $sz)
            $g.Dispose()

            $ms = New-Object System.IO.MemoryStream
            $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
            $bmp.Dispose()
            $streams += $ms
        }
        $src.Dispose()

        $fs = [System.IO.File]::Create("app.ico")
        $bw = New-Object System.IO.BinaryWriter $fs

        $bw.Write([uint16]0)
        $bw.Write([uint16]1)
        $bw.Write([uint16]$sizes.Count)

        $offset = 6 + (16 * $sizes.Count)

        for ($i = 0; $i -lt $sizes.Count; $i++) {
            $sz = $sizes[$i]
            $len = $streams[$i].Length
            $w = if ($sz -ge 256) { [byte]0 } else { [byte]$sz }
            $h = if ($sz -ge 256) { [byte]0 } else { [byte]$sz }

            $bw.Write($w)
            $bw.Write($h)
            $bw.Write([byte]0)
            $bw.Write([byte]0)
            $bw.Write([uint16]1)
            $bw.Write([uint16]32)
            $bw.Write([uint32]$len)
            $bw.Write([uint32]$offset)
            $offset += $len
        }

        foreach ($ms in $streams) {
            $bw.Write($ms.ToArray())
            $ms.Dispose()
        }

        $bw.Close()
        $fs.Close()
        Write-Host "app.ico успешно обновлен из icon.png"
    } catch {
        Write-Host "Не удалось создать app.ico: $_"
    }
}
