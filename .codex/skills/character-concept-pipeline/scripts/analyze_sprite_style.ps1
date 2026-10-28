param(
    [string]$Root = 'Assets/Art/ThirdParty/RPG/Characters(100x100)'
)

Add-Type -AssemblyName System.Drawing

$files = Get-ChildItem -LiteralPath $Root -Recurse -File -Filter '*-Idle.png' |
    Where-Object {
        $_.FullName -notmatch 'with shadows' -and
        $_.FullName -notmatch 'Split Effects' -and
        $_.Directory.Name -eq $_.Directory.Parent.Name
    } |
    Sort-Object FullName

foreach ($file in $files) {
    $bitmap = [System.Drawing.Bitmap]::FromFile($file.FullName)
    $frameCount = [Math]::Max(1, [Math]::Floor($bitmap.Width / 100))
    $frameStats = @()

    for ($frame = 0; $frame -lt $frameCount; $frame++) {
        $left = $frame * 100
        $minX = 100
        $minY = $bitmap.Height
        $maxX = -1
        $maxY = -1
        $colors = [System.Collections.Generic.HashSet[int]]::new()
        $edgeColors = @{}

        for ($y = 0; $y -lt [Math]::Min(100, $bitmap.Height); $y++) {
            for ($localX = 0; $localX -lt 100; $localX++) {
                $x = $left + $localX
                if ($x -ge $bitmap.Width) { continue }
                $pixel = $bitmap.GetPixel($x, $y)
                if ($pixel.A -eq 0) { continue }

                [void]$colors.Add($pixel.ToArgb())
                if ($localX -lt $minX) { $minX = $localX }
                if ($localX -gt $maxX) { $maxX = $localX }
                if ($y -lt $minY) { $minY = $y }
                if ($y -gt $maxY) { $maxY = $y }

                $isEdge = $false
                foreach ($offset in @(@(-1,0), @(1,0), @(0,-1), @(0,1))) {
                    $nx = $x + $offset[0]
                    $ny = $y + $offset[1]
                    if ($nx -lt $left -or $nx -ge ($left + 100) -or $ny -lt 0 -or $ny -ge $bitmap.Height -or $bitmap.GetPixel($nx, $ny).A -eq 0) {
                        $isEdge = $true
                        break
                    }
                }
                if ($isEdge) {
                    $hex = '#{0:X2}{1:X2}{2:X2}' -f $pixel.R, $pixel.G, $pixel.B
                    if (-not $edgeColors.ContainsKey($hex)) { $edgeColors[$hex] = 0 }
                    $edgeColors[$hex]++
                }
            }
        }

        if ($maxX -ge 0) {
            $dominantEdges = $edgeColors.GetEnumerator() |
                Sort-Object Value -Descending |
                Select-Object -First 3 |
                ForEach-Object { '{0}:{1}' -f $_.Key, $_.Value }
            $frameStats += [pscustomobject]@{
                Width = $maxX - $minX + 1
                Height = $maxY - $minY + 1
                Baseline = $maxY
                Colors = $colors.Count
                Edges = ($dominantEdges -join ',')
            }
        }
    }

    if ($frameStats.Count -gt 0) {
        [pscustomobject]@{
            Character = $file.Directory.Parent.Name
            Frames = $frameStats.Count
            WidthRange = '{0}-{1}' -f (($frameStats.Width | Measure-Object -Minimum).Minimum), (($frameStats.Width | Measure-Object -Maximum).Maximum)
            HeightRange = '{0}-{1}' -f (($frameStats.Height | Measure-Object -Minimum).Minimum), (($frameStats.Height | Measure-Object -Maximum).Maximum)
            BaselineRange = '{0}-{1}' -f (($frameStats.Baseline | Measure-Object -Minimum).Minimum), (($frameStats.Baseline | Measure-Object -Maximum).Maximum)
            ColorRange = '{0}-{1}' -f (($frameStats.Colors | Measure-Object -Minimum).Minimum), (($frameStats.Colors | Measure-Object -Maximum).Maximum)
            DominantBoundaryColors = $frameStats[0].Edges
            File = $file.FullName
        }
    }

    $bitmap.Dispose()
}
