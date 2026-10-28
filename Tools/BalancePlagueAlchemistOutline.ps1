param(
    [string[]]$Paths = @(
        'Assets/Art/Concepts/PlagueAlchemist/PlagueAlchemist-Idle-Reference.png',
        'Assets/Art/Generated/Units/PlagueAlchemist/PlagueAlchemist-Idle.png',
        'Assets/Art/Generated/Units/PlagueAlchemist/PlagueAlchemist-Move.png',
        'Assets/Art/Generated/Units/PlagueAlchemist/PlagueAlchemist-Attack.png',
        'Assets/Art/Generated/Units/PlagueAlchemist/PlagueAlchemist-Die.png'
    )
)

Add-Type -AssemblyName System.Drawing
$outline = [System.Drawing.Color]::FromArgb(255, 31, 26, 26)

function Is-Outline([System.Drawing.Color]$color) {
    return $color.A -eq 255 -and $color.R -eq 31 -and $color.G -eq 26 -and $color.B -eq 26
}

function Get-MaterialEdgeColor([System.Drawing.Color]$neighbor) {
    if ($neighbor.G -gt ($neighbor.R * 1.08) -and $neighbor.G -gt ($neighbor.B * 1.15)) {
        return [System.Drawing.Color]::FromArgb(255, 60, 61, 37)  # olive shadow
    }
    if ($neighbor.R -gt ($neighbor.G * 1.22)) {
        return [System.Drawing.Color]::FromArgb(255, 76, 45, 39)  # leather shadow
    }
    if ($neighbor.R -gt 105 -and $neighbor.G -gt 80) {
        return [System.Drawing.Color]::FromArgb(255, 106, 74, 34) # brass/liquid shadow
    }
    return [System.Drawing.Color]::FromArgb(255, 76, 69, 65)     # charcoal shadow
}

foreach ($path in $Paths) {
    $resolved = Resolve-Path -LiteralPath $path
    $bitmap = [System.Drawing.Bitmap]::FromFile($resolved)
    $frameCount = [Math]::Max(1, [Math]::Floor($bitmap.Width / 100))

    for ($frame = 0; $frame -lt $frameCount; $frame++) {
        $cellX = $frame * 100
        $edgeCount = 0
        $candidates = [System.Collections.Generic.List[object]]::new()

        for ($y = 0; $y -lt 100; $y++) {
            for ($localX = 0; $localX -lt 100; $localX++) {
                $x = $cellX + $localX
                $pixel = $bitmap.GetPixel($x, $y)
                if ($pixel.A -eq 0) { continue }

                $transparentTop = $y -eq 0 -or $bitmap.GetPixel($x, $y - 1).A -eq 0
                $transparentLeft = $localX -eq 0 -or $bitmap.GetPixel($x - 1, $y).A -eq 0
                $transparentRight = $localX -eq 99 -or $bitmap.GetPixel($x + 1, $y).A -eq 0
                $transparentBottom = $y -eq 99 -or $bitmap.GetPixel($x, $y + 1).A -eq 0
                if (-not ($transparentTop -or $transparentLeft -or $transparentRight -or $transparentBottom)) { continue }
                $edgeCount++
                if (-not (Is-Outline $pixel) -or -not ($transparentTop -or $transparentLeft)) { continue }

                $bestNeighbor = $null
                $bestLuminance = -1.0
                foreach ($offset in @(@(-1,0), @(1,0), @(0,-1), @(0,1), @(-1,-1), @(1,-1), @(-1,1), @(1,1))) {
                    $nx = $x + $offset[0]
                    $ny = $y + $offset[1]
                    if ($nx -lt $cellX -or $nx -ge ($cellX + 100) -or $ny -lt 0 -or $ny -ge 100) { continue }
                    $neighbor = $bitmap.GetPixel($nx, $ny)
                    if ($neighbor.A -eq 0 -or (Is-Outline $neighbor)) { continue }
                    $luminance = (0.2126 * $neighbor.R) + (0.7152 * $neighbor.G) + (0.0722 * $neighbor.B)
                    if ($luminance -gt $bestLuminance) {
                        $bestLuminance = $luminance
                        $bestNeighbor = $neighbor
                    }
                }
                if ($null -ne $bestNeighbor) {
                    $candidates.Add([pscustomobject]@{
                        X = $x
                        Y = $y
                        Score = $bestLuminance + $(if ($transparentTop) { 20 } else { 0 })
                        Color = Get-MaterialEdgeColor $bestNeighbor
                    })
                }
            }
        }

        # Purchased references retain #1F1A1A on roughly 78-92% of exposed edges.
        # Recolor only the brightest upper-left 15%, keeping the shared outline dominant.
        $recolorCount = [Math]::Min($candidates.Count, [Math]::Round($edgeCount * 0.15))
        $selected = $candidates | Sort-Object Score -Descending | Select-Object -First $recolorCount
        foreach ($candidate in $selected) {
            $bitmap.SetPixel($candidate.X, $candidate.Y, $candidate.Color)
        }
    }

    $temporary = "$resolved.tmp.png"
    $bitmap.Save($temporary, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
    Move-Item -LiteralPath $temporary -Destination $resolved -Force
    Write-Output "Balanced outline: $path"
}
