# TODO @gregolsky update regexes for stable
$CATEGORIES = @(
    @('RavenDB-[0-9]\.[0-9]\.[0-9]-[a-zA-Z]+-([0-9-]+)-windows-x64', "RavenDB for Windows x64"),
    @('RavenDB-[0-9]\.[0-9]\.[0-9]-[a-zA-Z]+-([0-9-]+)-windows-x86', "RavenDB for Windows x86"),
    @('RavenDB-[0-9]\.[0-9]\.[0-9]-[a-zA-Z]+-([0-9-]+)-ubuntu\.14\.04-x64', "RavenDB for Ubuntu 14.04 x64"),
    @('RavenDB-[0-9]\.[0-9]\.[0-9]-[a-zA-Z]+-([0-9-]+)-ubuntu\.16\.04-x64', "RavenDB for Ubuntu 16.04 x64"),
    @('RavenDB-[0-9]\.[0-9]\.[0-9]-[a-zA-Z]+-([0-9-]+)-raspberry-pi', "RavenDB for Raspberry Pi")
)
function Get-UploadCategory ( $filename ) {
    $result = [io.path]::GetFilenameWithoutExtension($filename)
    foreach ($category in $CATEGORIES) {
        $categoryPattern = $category[0]
        if ($filename -match $categoryPattern) {
            $result = $category[1]
            break
        }
    }

    $result
}

function FormatBuildDownloadVersion($versionInfo) {
    $buildNumber = $versionInfo.BuildNumber
    $builtAtString = $versionInfo.BuiltAtString
    $buildType = $versionInfo.BuildType

    if ($buildType.ToLower() -eq 'nightly') {
        $versionString = "$builtAtString-$((Get-Culture).textinfo.toTitleCase($buildType))"
    } else {
        $versionString = "$buildNumber-$((Get-Culture).textinfo.toTitleCase($buildType))"
    }

    return $versionString
}
function UploadArtifact ($uploader, $versionInfo, $filename, $log, $dryRun) {
    $uploadCategory = Get-UploadCategory $filename
    $versionString = FormatBuildDownloadVersion($versionInfo)

    write-host "Executing: $uploader ""$uploadCategory"" ""$versionString"" $filename ""$log"""
    $uploadTryCount = 0
    while ($uploadTryCount -lt 5) {
        $uploadTryCount += 1

        if ($dryRun -eq $False) {
            & $uploader "$uploadCategory" "$versionString" "$filename" "$log"
        }

        if ($lastExitCode -ne 0) {
            write-host "Failed to upload to S3: $lastExitCode. UploadTryCount: $uploadTryCount"
        }
        else {
            break
        }
    }

    if ($lastExitCode -ne 0) {
        write-host "Failed to upload to S3: $lastExitCode. UploadTryCount: $uploadTryCount. Build will fail."
        throw "Error: Failed to publish build"
    }
}

function Upload ($uploader, $versionInfo, $artifacts, $dryRun) {
    Write-Host "Starting upload"

    if (($dryRun -eq $False) -and $(Test-Path $uploader) -eq $False) {
        throw "$uploader not found."
    }

    $log = git log -n 1 --oneline
    $log = $log.Replace('"','''') # avoid problems because of " escaping the output

    foreach ($filename in $artifacts)
    {
        UploadArtifact $uploader $versionInfo $filename $log $dryRun
    }
}