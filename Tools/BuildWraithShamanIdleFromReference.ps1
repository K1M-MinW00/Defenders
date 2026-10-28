param(
    [Parameter(Mandatory = $true)][string]$Reference,
    [Parameter(Mandatory = $true)][string]$Output,
    [Parameter(Mandatory = $true)][string]$Preview
)

Add-Type -AssemblyName System.Drawing

$source = [System.Drawing.Bitmap]::FromFile($Reference)
try {
    if ($source.Width -ne 100 -or $source.Height -ne 100) {
        throw "Expected a 100x100 approved Idle reference, found $($source.Width)x$($source.Height)."
    }

    # Existing project Idles change the silhouette around a stable lower anchor:
    # inhale expands the body by 1px steps, exhale compresses it by 1px steps.
    # This redraws the pixel rows instead of translating six identical copies.
    $targetHeights = @(31, 32, 32, 31, 30, 30)
    $tailOffsets = @(0, 1, 1, 0, -1, -1)
    $flameOffsets = @(0, 0, 1, 1, 0, -1)
    $sourceBounds = New-Object System.Drawing.Rectangle 38, 25, 23, 31
    $fixedBottom = 56
    $sheet = New-Object System.Drawing.Bitmap 600, 100, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($sheet)
        try {
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
            for ($frame = 0; $frame -lt 6; $frame++) {
                $height = $targetHeights[$frame]
                $target = New-Object System.Drawing.Rectangle ($frame * 100 + 38), ($fixedBottom - $height), 23, $height
                $graphics.DrawImage($source, $target, $sourceBounds, [System.Drawing.GraphicsUnit]::Pixel)
            }
        }
        finally { $graphics.Dispose() }

        for ($y = 0; $y -lt 100; $y++) {
            for ($x = 0; $x -lt 600; $x++) {
                $color = $sheet.GetPixel($x, $y)
                if ($color.A -lt 128) { $sheet.SetPixel($x, $y, [System.Drawing.Color]::Transparent) }
                elseif ($color.A -ne 255) { $sheet.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $color.R, $color.G, $color.B)) }
            }
        }

        # Add the same kind of one-pixel secondary follow-through seen in the
        # project's Wizard and armored monster Idles. Body, tail, and flame peak
        # on slightly different frames, while the face and staff remain locked.
        for ($frame = 0; $frame -lt 6; $frame++) {
            $cellX = $frame * 100

            $tailPixels = New-Object System.Collections.Generic.List[object]
            for ($y = 45; $y -le 56; $y++) {
                for ($x = 38; $x -le 60; $x++) {
                    $color = $sheet.GetPixel($cellX + $x, $y)
                    if ($color.A -eq 0) { continue }
                    $tailPixels.Add(@($x, $y, $color))
                    $sheet.SetPixel($cellX + $x, $y, [System.Drawing.Color]::Transparent)
                }
            }
            foreach ($pixel in $tailPixels) {
                $sheet.SetPixel($cellX + $pixel[0] + $tailOffsets[$frame], $pixel[1], $pixel[2])
            }

            $flamePixels = New-Object System.Collections.Generic.List[object]
            for ($y = 20; $y -le 34; $y++) {
                for ($x = 36; $x -le 47; $x++) {
                    $color = $sheet.GetPixel($cellX + $x, $y)
                    if ($color.A -eq 0 -or $color.B -lt 120 -or $color.G -lt 80) { continue }
                    $flamePixels.Add(@($x, $y, $color))
                    $sheet.SetPixel($cellX + $x, $y, [System.Drawing.Color]::Transparent)
                }
            }
            foreach ($pixel in $flamePixels) {
                $sheet.SetPixel($cellX + $pixel[0] + $flameOffsets[$frame], $pixel[1], $pixel[2])
            }
        }

        $sheet.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)

        $previewImage = New-Object System.Drawing.Bitmap 2400, 400, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $graphics = [System.Drawing.Graphics]::FromImage($previewImage)
            try {
                $graphics.Clear([System.Drawing.Color]::FromArgb(34, 37, 44))
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
                $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
                $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
                $graphics.DrawImage($sheet, 0, 0, $previewImage.Width, $previewImage.Height)
            }
            finally { $graphics.Dispose() }
            $previewImage.Save($Preview, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $previewImage.Dispose() }

        for ($frame = 0; $frame -lt 6; $frame++) {
            $minX = 100; $minY = 100; $maxX = -1; $maxY = -1
            for ($y = 0; $y -lt 100; $y++) {
                for ($x = 0; $x -lt 100; $x++) {
                    if ($sheet.GetPixel($frame * 100 + $x, $y).A -eq 0) { continue }
                    if ($x -lt $minX) { $minX = $x }; if ($x -gt $maxX) { $maxX = $x }
                    if ($y -lt $minY) { $minY = $y }; if ($y -gt $maxY) { $maxY = $y }
                }
            }
            "frame=$frame targetHeight=$($targetHeights[$frame]) width=$($maxX-$minX+1) height=$($maxY-$minY+1)"
        }
    }
    finally { $sheet.Dispose() }
}
finally { $source.Dispose() }
