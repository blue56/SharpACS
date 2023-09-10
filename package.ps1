$date = Get-Date
$version = $date.ToString("yyyy-dd-M--HH-mm-ss")
$filename = "SharpACS-" + $version + ".zip"
cd .\SharpACS\src\SharpACS
dotnet lambda package ..\..\..\Packages\$filename -frun dotnet6 -farch arm64
cd ..\..\..