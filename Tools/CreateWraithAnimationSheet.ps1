param(
    [Parameter(Mandatory = $true)][string]$Source,
    [Parameter(Mandatory = $true)][ValidateRange(2, 16)][int]$FrameCount,
    [Parameter(Mandatory = $true)][string]$Output,
    [Parameter(Mandatory = $true)][string]$Preview
)

Add-Type -AssemblyName System.Drawing

$palette = @(
    '#1F1A1A', '#302D36', '#45424C', '#5D5963',
    '#303958', '#414E72', '#556482', '#667895',
    '#425C6B', '#607581', '#7F909A', '#9BADB7',
    '#4C3029', '#694133', '#865847',
    '#8F887A', '#B7A88E', '#D0C7B5',
    '#88A9AD', '#A7C8C8'
) | ForEach-Object { [System.Drawing.ColorTranslator]::FromHtml($_) }

function Find-NearestColor([System.Drawing.Color]$Color) {
    $best = $palette[0]; $bestDistance = [double]::MaxValue
    foreach ($candidate in $palette) {
        $dr = [int]$Color.R - [int]$candidate.R
        $dg = [int]$Color.G - [int]$candidate.G
        $db = [int]$Color.B - [int]$candidate.B
        $distance = 0.30 * $dr * $dr + 0.59 * $dg * $dg + 0.11 * $db * $db
        if ($distance -lt $bestDistance) { $bestDistance = $distance; $best = $candidate }
    }
    return $best
}

$input = [System.Drawing.Bitmap]::FromFile($Source)
try {
    $sheet = New-Object System.Drawing.Bitmap ($FrameCount * 100), 100, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($sheet)
        try {
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality

            for ($frame = 0; $frame -lt $FrameCount; $frame++) {
                $segmentStart = [int][Math]::Floor($frame * $input.Width / [double]$FrameCount)
                $segmentEnd = [int][Math]::Floor(($frame + 1) * $input.Width / [double]$FrameCount)
                $minX = $segmentEnd; $minY = $input.Height; $maxX = -1; $maxY = -1
                for ($y = 0; $y -lt $input.Height; $y++) {
                    for ($x = $segmentStart; $x -lt $segmentEnd; $x++) {
                        if ($input.GetPixel($x, $y).A -lt 24) { continue }
                        $minX = [Math]::Min($minX, $x); $maxX = [Math]::Max($maxX, $x)
                        $minY = [Math]::Min($minY, $y); $maxY = [Math]::Max($maxY, $y)
                    }
                }
                if ($maxX -lt 0) { throw "Frame $frame has no opaque pixels." }
                $src = New-Object System.Drawing.Rectangle $minX, $minY, ($maxX - $minX + 1), ($maxY - $minY + 1)
                $dst = New-Object System.Drawing.Rectangle ($frame * 100 + 38), 27, 24, 29
                $graphics.DrawImage($input, $dst, $src, [System.Drawing.GraphicsUnit]::Pixel)
            }
        }
        finally { $graphics.Dispose() }

        for ($y = 0; $y -lt 100; $y++) {
            for ($x = 0; $x -lt $sheet.Width; $x++) {
                $color = $sheet.GetPixel($x, $y)
                if ($color.A -lt 112) { $sheet.SetPixel($x, $y, [System.Drawing.Color]::Transparent); continue }
                $mapped = Find-NearestColor $color
                $sheet.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $mapped.R, $mapped.G, $mapped.B))
            }
        }

        # Remove only isolated one-pixel generation debris; keep connected highlights
        # and every intentional part of the silhouette.
        $snapshot = $sheet.Clone()
        try {
            for ($frame = 0; $frame -lt $FrameCount; $frame++) {
                for ($y = 1; $y -lt 99; $y++) {
                    for ($localX = 1; $localX -lt 99; $localX++) {
                        $x = $frame * 100 + $localX
                        if ($snapshot.GetPixel($x, $y).A -eq 0) { continue }
                        $neighbors = 0
                        for ($oy = -1; $oy -le 1; $oy++) {
                            for ($ox = -1; $ox -le 1; $ox++) {
                                if ($ox -eq 0 -and $oy -eq 0) { continue }
                                if ($snapshot.GetPixel($x + $ox, $y + $oy).A -gt 0) { $neighbors++ }
                            }
                        }
                        if ($neighbors -eq 0) { $sheet.SetPixel($x, $y, [System.Drawing.Color]::Transparent) }
                    }
                }
            }
        }
        finally { $snapshot.Dispose() }

        for ($frame = 0; $frame -lt $FrameCount; $frame++) {
            $visited = New-Object 'bool[,]' 100, 100
            $components = New-Object System.Collections.Generic.List[object]
            for ($localY = 0; $localY -lt 100; $localY++) {
                for ($localX = 0; $localX -lt 100; $localX++) {
                    if ($visited[$localX, $localY]) { continue }
                    $visited[$localX, $localY] = $true
                    if ($sheet.GetPixel($frame * 100 + $localX, $localY).A -eq 0) { continue }
                    $queue = New-Object System.Collections.Generic.Queue[object]
                    $component = New-Object System.Collections.Generic.List[object]
                    $queue.Enqueue(@($localX, $localY))
                    while ($queue.Count -gt 0) {
                        $pixel = $queue.Dequeue(); $component.Add($pixel)
                        for ($oy = -1; $oy -le 1; $oy++) {
                            for ($ox = -1; $ox -le 1; $ox++) {
                                if ($ox -eq 0 -and $oy -eq 0) { continue }
                                $nx = $pixel[0] + $ox; $ny = $pixel[1] + $oy
                                if ($nx -lt 0 -or $nx -ge 100 -or $ny -lt 0 -or $ny -ge 100 -or $visited[$nx, $ny]) { continue }
                                $visited[$nx, $ny] = $true
                                if ($sheet.GetPixel($frame * 100 + $nx, $ny).A -gt 0) { $queue.Enqueue(@($nx, $ny)) }
                            }
                        }
                    }
                    $components.Add($component)
                }
            }
            if ($components.Count -gt 0) {
                $largest = $components | Sort-Object Count -Descending | Select-Object -First 1
                foreach ($component in $components) {
                    if ([object]::ReferenceEquals($component, $largest)) { continue }
                    foreach ($pixel in $component) {
                        $sheet.SetPixel($frame * 100 + $pixel[0], $pixel[1], [System.Drawing.Color]::Transparent)
                    }
                }
            }
        }
        $sheet.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)

        $previewImage = New-Object System.Drawing.Bitmap ($sheet.Width * 4), 400, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $previewGraphics = [System.Drawing.Graphics]::FromImage($previewImage)
            try {
                $previewGraphics.Clear([System.Drawing.Color]::FromArgb(145, 72, 55))
                $previewGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
                $previewGraphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
                $previewGraphics.DrawImage($sheet, 0, 0, $previewImage.Width, $previewImage.Height)
            }
            finally { $previewGraphics.Dispose() }
            $previewImage.Save($Preview, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $previewImage.Dispose() }
    }
    finally { $sheet.Dispose() }
}
finally { $input.Dispose() }
