dotnet publish src/AiHands/AiHands.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -o ./publish

$exe = Get-Item ./publish/ai-hands.exe
Write-Host "Published to $($exe.FullName)"
Write-Host "Size: $([math]::Round($exe.Length / 1MB, 1)) MB"
