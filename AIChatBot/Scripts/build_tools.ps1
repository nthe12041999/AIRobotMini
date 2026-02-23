# Build Python scripts to standalone .exe using PyInstaller
# Requires: python -m pip install pyinstaller youtube-transcript-api edge-tts

$ErrorActionPreference = 'Stop'

Write-Host "Building Python scripts to standalone .exe..."

# Install dependencies using python -m pip (more reliable)
Write-Host "Installing Python dependencies..."
python -m pip install pyinstaller youtube-transcript-api edge-tts

$scriptDir = $PSScriptRoot
$toolsDir = Join-Path (Split-Path $scriptDir -Parent) "Tools"

if (-not (Test-Path $toolsDir)) {
    New-Item -ItemType Directory -Path $toolsDir -Force | Out-Null
}

# Build get_transcript.py
Write-Host "Building get_transcript.exe..."
$getTranscriptPy = Join-Path $scriptDir "get_transcript.py"
if (Test-Path $getTranscriptPy) {
    python -m PyInstaller --onefile --distpath $toolsDir --workpath "$env:TEMP\pyinstaller" --specpath "$env:TEMP\pyinstaller" --clean --noconfirm $getTranscriptPy
    Write-Host "Created: $toolsDir\get_transcript.exe"
}

# Build list_transcript_languages.py
Write-Host "Building list_transcript_languages.exe..."
$listLangPy = Join-Path $scriptDir "list_transcript_languages.py"
if (Test-Path $listLangPy) {
    python -m PyInstaller --onefile --distpath $toolsDir --workpath "$env:TEMP\pyinstaller" --specpath "$env:TEMP\pyinstaller" --clean --noconfirm $listLangPy
    Write-Host "Created: $toolsDir\list_transcript_languages.exe"
}

# Build edge_tts_convert.py
Write-Host "Building edge_tts_convert.exe..."
$edgeTtsPy = Join-Path $scriptDir "edge_tts_convert.py"
if (Test-Path $edgeTtsPy) {
    python -m PyInstaller --onefile --distpath $toolsDir --workpath "$env:TEMP\pyinstaller" --specpath "$env:TEMP\pyinstaller" --clean --noconfirm --hidden-import edge_tts $edgeTtsPy
    Write-Host "Created: $toolsDir\edge_tts_convert.exe"
}

Write-Host "`nDone! .exe files are in: $toolsDir"
Write-Host "Copy the Tools folder to your app distribution."
