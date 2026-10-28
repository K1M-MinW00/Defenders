param(
    [Parameter(Mandatory = $true)][string]$Source,
    [Parameter(Mandatory = $true)][string]$Output,
    [Parameter(Mandatory = $true)][string]$Preview
)

Add-Type -AssemblyName System.Drawing

$input = [System.Drawing.Bitmap]::FromFile($Source)
try {
    $sheet = New-Object System.Drawing.Bitmap 600, 100, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($sheet)
        try {
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighSpeed
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None

            for ($frame = 0; $frame -lt 6; $frame++) {
                $startX = [int][Math]::Floor($frame * $input.Width / 6.0)
                $endX = [int][Math]::Floor(($frame + 1) * $input.Width / 6.0)
                $minX = $endX; $minY = $input.Height; $maxX = -1; $maxY = -1
                for ($y = 0; $y -lt $input.Height; $y++) {
                    for ($x = $startX; $x -lt $endX; $x++) {
                        if ($input.GetPixel($x, $y).A -lt 48) { continue }
                        $minX = [Math]::Min($minX, $x); $maxX = [Math]::Max($maxX, $x)
                        $minY = [Math]::Min($minY, $y); $maxY = [Math]::Max($maxY, $y)
                    }
                }
                if ($maxX -lt 0) { throw "Frame $frame contains no opaque pixels." }

                $sourceWidth = $maxX - $minX + 1
                $sourceHeight = $maxY - $minY + 1
                $targetHeight = 30
                $targetWidth = [int][Math]::Round($sourceWidth * ($targetHeight / [double]$sourceHeight))
                if ($targetWidth -gt 27) { $targetWidth = 27 }
                $targetX = $frame * 100 + [int][Math]::Floor((100 - $targetWidth) / 2)
                $targetY = 27
                $srcRect = New-Object System.Drawing.Rectangle $minX, $minY, $sourceWidth, $sourceHeight
                $dstRect = New-Object System.Drawing.Rectangle $targetX, $targetY, $targetWidth, $targetHeight
                $graphics.DrawImage($input, $dstRect, $srcRect, [System.Drawing.GraphicsUnit]::Pixel)
            }
        }
        finally { $graphics.Dispose() }

        # Hard alpha keeps the result consistent with the existing point-filtered sheets.
        for ($y = 0; $y -lt 100; $y++) {
            for ($x = 0; $x -lt 600; $x++) {
                $color = $sheet.GetPixel($x, $y)
                if ($color.A -lt 112) { $sheet.SetPixel($x, $y, [System.Drawing.Color]::Transparent) }
                elseif ($color.A -ne 255) { $sheet.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $color.R, $color.G, $color.B)) }
            }
        }
        $sheet.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)

        $large = New-Object System.Drawing.Bitmap 2400, 400, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $g = [System.Drawing.Graphics]::FromImage($large)
            try {
                $g.Clear([System.Drawing.Color]::FromArgb(34, 37, 44))
                $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
                $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
                $g.DrawImage($sheet, 0, 0, $large.Width, $large.Height)
            }
            finally { $g.Dispose() }
            $large.Save($Preview, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $large.Dispose() }
    }
    finally { $sheet.Dispose() }
}
finally { $input.Dispose() }
