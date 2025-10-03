# Run the app with auto-rebuild on code changes
# Usage: pwsh -File dev.ps1

$ErrorActionPreference = "Stop"

Write-Host "Starting dotnet watch... (Ctrl+C to stop)" -ForegroundColor Cyan

dotnet watch run