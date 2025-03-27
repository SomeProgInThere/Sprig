param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath
)

# Clean up previous build artifacts
$directory = Split-Path -Parent $FilePath
Remove-Item -Path "$directory/*.txt" -ErrorAction SilentlyContinue
Remove-Item -Path "$directory/*.dot" -ErrorAction SilentlyContinue
Remove-Item -Path "$directory/*.exe" -ErrorAction SilentlyContinue

# Build the project with the specified file
dotnet run --project Twig build $FilePath