# Chép skill từ .agents/skills (bản gốc, Codex và Gemini CLI đọc)
# sang .claude/skills (Claude Code đọc).
# Chạy từ gốc repo:  powershell -ExecutionPolicy Bypass -File scripts/sync-skills.ps1

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$src = Join-Path $root '.agents\skills'
$dst = Join-Path $root '.claude\skills'

if (-not (Test-Path $src)) { throw "Không tìm thấy $src" }
New-Item -ItemType Directory -Force -Path $dst | Out-Null

Get-ChildItem -Path $src -Directory | ForEach-Object {
    $target = Join-Path $dst $_.Name
    if (Test-Path $target) { Remove-Item -Recurse -Force $target }
    Copy-Item -Recurse -Path $_.FullName -Destination $target
    Write-Host "Đã chép $($_.Name)"
}

# Xóa skill đã bị xóa khỏi bản gốc
Get-ChildItem -Path $dst -Directory | Where-Object { -not (Test-Path (Join-Path $src $_.Name)) } | ForEach-Object {
    Remove-Item -Recurse -Force $_.FullName
    Write-Host "Đã xóa $($_.Name)"
}
