# Specify the output directory
$outputDirectory = "downloaded_data"

Function Download-And-Save-File {
    Param(
        $url,
        $outputDirectory
    )

    $fileName = [System.IO.Path]::GetFileName($url)
    $outputPath = Join-Path $outputDirectory $fileName

    Write-Host "Downloading $url to $outputPath"
    Invoke-WebRequest -Uri $url -OutFile $outputPath
}

# Set the starting point
$startMonth = 8
$startDay = 16
$startHour = 20

# Loop from the starting point
$startMonth..12 | ForEach-Object { # Loop for months
    $month = $_
    $monthFormatted = $month.ToString('D2')

    # Set the starting day for the current month
    $startDayOfMonth = if ($month -eq $startMonth) { $startDay } else { 1 }

    $startDayOfMonth..31 | ForEach-Object { # Loop for days
        $day = $_
        $dayFormatted = $day.ToString('D2')

        # Set the starting hour for the current day
        $startHourOfDay = if ($month -eq $startMonth -and $day -eq $startDay) { $startHour } else { 0 }

        $startHourOfDay..23 | ForEach-Object { # Loop for hours
            $hour = $_
            $url = "https://data.gharchive.org/2023-$monthFormatted-$dayFormatted-$hour.json.gz"
            
            # Download file
            Download-And-Save-File -url $url -outputDirectory $outputDirectory
        }
    }
}

Write-Host "Download complete. Files saved to $outputDirectory"