# Build Python scripts to standalone .exe files
$ErrorActionPreference = 'Stop'

Write-Host "=== Building Python scripts to .exe ===" -ForegroundColor Cyan

# Check if pip and pyinstaller are available
Write-Host "Checking PyInstaller..."
$hasInstaller = pip show pyinstaller 2>$null
if (-not $hasInstaller) {
    Write-Host "Installing PyInstaller..."
    pip install pyinstaller
}

# Create Tools folder if not exists
$toolsDir = "..\Tools"
if (-not (Test-Path $toolsDir)) {
    New-Item -ItemType Directory -Path $toolsDir -Force | Out-Null
}

# Build get_transcript.py
Write-Host "Building get_transcript.exe..."
pyinstaller --onefile --clean --distpath "$toolsDir" --workpath ".\build" --specpath ".\build" get_transcript.py

# Build list_transcript_languages.py  
Write-Host "Building list_transcript_languages.exe..."
pyinstaller --onefile --clean --distpath "$toolsDir" --workpath ".\build" --specpath ".\build" list_transcript_languages.py

# Build edge_tts script
Write-Host "Building edge_tts_convert.exe..."
pyinstaller --onefile --clean --distpath "$toolsDir" --workpath ".\build" --specpath ".\build" edge_tts_convert.py

# Cleanup build folders
if (Test-Path ".\build") { Remove-Item ".\build" -Recurse -Force }

Write-Host ""
Write-Host "=== Done! ===" -ForegroundColor Green
Write-Host "Files created in Tools folder:"
Get-ChildItem $toolsDir -Filter "*.exe" | ForEach-Object { Write-Host "  - $($_.Name)" }
