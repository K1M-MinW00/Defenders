param(
    [Parameter(Mandatory = $true)][string]$Source,
    [Parameter(Mandatory = $true)][string]$Output,
    [Parameter(Mandatory = $true)][string]$Preview
)

Add-Type -AssemblyName System.Drawing

$sourceImage = [System.Drawing.Bitmap]::FromFile($Source)
try {
    $frameCount = 6
    $cellWidth = 100
    $cellHeight = 100
    $sourceCellWidth = [int]($sourceImage.Width / $frameCount)
    $bounds = @()

    for ($frame = 0; $frame -lt $frameCount; $frame++) {
        $minX = $sourceImage.Width
        $minY = $sourceImage.Height
        $maxX = -1
        $maxY = -1
        $startX = $frame * $sourceCellWidth
        $endX = if ($frame -eq $frameCount - 1) { $sourceImage.Width } else { ($frame + 1) * $sourceCellWidth }

        for ($y = 0; $y -lt $sourceImage.Height; $y++) {
            for ($x = $startX; $x -lt $endX; $x++) {
                if ($sourceImage.GetPixel($x, $y).A -ge 32) {
                    $localX = $x - $startX
                    if ($localX -lt $minX) { $minX = $localX }
                    if ($localX -gt $maxX) { $maxX = $localX }
                    if ($y -lt $minY) { $minY = $y }
                    if ($y -gt $maxY) { $maxY = $y }
                }
            }
        }

        if ($maxX -lt 0) { throw "Frame $frame has no opaque pixels." }
        $bounds += [pscustomobject]@{ X = $minX; Y = $minY; Width = $maxX - $minX + 1; Height = $maxY - $minY + 1 }
    }

    $maxSourceWidth = ($bounds | Measure-Object Width -Maximum).Maximum
    $maxSourceHeight = ($bounds | Measure-Object Height -Maximum).Maximum
    $scale = [Math]::Min(30.0 / $maxSourceHeight, 34.0 / $maxSourceWidth)

    $sheet = New-Object System.Drawing.Bitmap ($cellWidth * $frameCount), $cellHeight, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($sheet)
        try {
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighSpeed
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None

            for ($frame = 0; $frame -lt $frameCount; $frame++) {
                $b = $bounds[$frame]
                $targetWidth = [Math]::Max(1, [int][Math]::Round($b.Width * $scale))
                $targetHeight = [Math]::Max(1, [int][Math]::Round($b.Height * $scale))
                $targetX = $frame * $cellWidth + [int][Math]::Floor(($cellWidth - $targetWidth) / 2)
                $targetY = 58 - $targetHeight
                $srcRect = New-Object System.Drawing.Rectangle (($frame * $sourceCellWidth) + $b.X), $b.Y, $b.Width, $b.Height
                $dstRect = New-Object System.Drawing.Rectangle $targetX, $targetY, $targetWidth, $targetHeight
                $graphics.DrawImage($sourceImage, $dstRect, $srcRect, [System.Drawing.GraphicsUnit]::Pixel)
            }
        }
        finally { $graphics.Dispose() }

        for ($y = 0; $y -lt $sheet.Height; $y++) {
            for ($x = 0; $x -lt $sheet.Width; $x++) {
                $c = $sheet.GetPixel($x, $y)
                if ($c.A -lt 96) { $sheet.SetPixel($x, $y, [System.Drawing.Color]::Transparent) }
                elseif ($c.A -ne 255) { $sheet.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $c.R, $c.G, $c.B)) }
            }
        }

        # Remove tiny disconnected generation artifacts without touching the main silhouette.
        for ($frame = 0; $frame -lt $frameCount; $frame++) {
            $visited = New-Object 'bool[,]' $cellWidth, $cellHeight
            $components = @()
            for ($localY = 0; $localY -lt $cellHeight; $localY++) {
                for ($localX = 0; $localX -lt $cellWidth; $localX++) {
                    if ($visited[$localX, $localY]) { continue }
                    $globalX = $frame * $cellWidth + $localX
                    if ($sheet.GetPixel($globalX, $localY).A -eq 0) { $visited[$localX, $localY] = $true; continue }
                    $queue = New-Object System.Collections.Generic.Queue[object]
                    $pixels = New-Object System.Collections.Generic.List[object]
                    $queue.Enqueue(@($localX, $localY)); $visited[$localX, $localY] = $true
                    while ($queue.Count -gt 0) {
                        $p = $queue.Dequeue(); $pixels.Add($p)
                        for ($oy = -1; $oy -le 1; $oy++) {
                            for ($ox = -1; $ox -le 1; $ox++) {
                                if ($ox -eq 0 -and $oy -eq 0) { continue }
                                $nx = $p[0] + $ox; $ny = $p[1] + $oy
                                if ($nx -lt 0 -or $nx -ge $cellWidth -or $ny -lt 0 -or $ny -ge $cellHeight -or $visited[$nx, $ny]) { continue }
                                $visited[$nx, $ny] = $true
                                if ($sheet.GetPixel($frame * $cellWidth + $nx, $ny).A -gt 0) { $queue.Enqueue(@($nx, $ny)) }
                            }
                        }
                    }
                    $components += ,$pixels
                }
            }
            if ($components.Count -gt 0) {
                $largest = $components | Sort-Object Count -Descending | Select-Object -First 1
                foreach ($component in $components) {
                    if ([object]::ReferenceEquals($component, $largest)) { continue }
                    foreach ($p in $component) { $sheet.SetPixel($frame * $cellWidth + $p[0], $p[1], [System.Drawing.Color]::Transparent) }
                }
            }
        }

        $sheet.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)

        $previewImage = New-Object System.Drawing.Bitmap ($sheet.Width * 4), ($sheet.Height * 4), ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $previewGraphics = [System.Drawing.Graphics]::FromImage($previewImage)
            try {
                $previewGraphics.Clear([System.Drawing.Color]::FromArgb(34, 37, 44))
                $previewGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
                $previewGraphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
                $previewGraphics.DrawImage($sheet, 0, 0, $previewImage.Width, $previewImage.Height)
            }
            finally { $previewGraphics.Dispose() }
            $previewImage.Save($Preview, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $previewImage.Dispose() }

        $result = @()
        for ($frame = 0; $frame -lt $frameCount; $frame++) {
            $minX = $sheet.Width; $minY = $sheet.Height; $maxX = -1; $maxY = -1
            for ($y = 0; $y -lt $sheet.Height; $y++) {
                for ($x = $frame * $cellWidth; $x -lt (($frame + 1) * $cellWidth); $x++) {
                    if ($sheet.GetPixel($x, $y).A -gt 0) {
                        if ($x -lt $minX) { $minX = $x }
                        if ($x -gt $maxX) { $maxX = $x }
                        if ($y -lt $minY) { $minY = $y }
                        if ($y -gt $maxY) { $maxY = $y }
                    }
                }
            }
            $result += "frame=$frame width=$($maxX-$minX+1) height=$($maxY-$minY+1) worldWidth=$([Math]::Round(($maxX-$minX+1)/16.0,3)) worldHeight=$([Math]::Round(($maxY-$minY+1)/16.0,3))"
        }
        $result
    }
    finally { $sheet.Dispose() }
}
finally { $sourceImage.Dispose() }
