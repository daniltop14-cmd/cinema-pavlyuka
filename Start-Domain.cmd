@echo off
setlocal
chcp 65001 >nul
set "CINEMA_ROOT=C:\Users\Admin\Documents\Codex\2026-11-05\c"
set "ASPNETCORE_ENVIRONMENT=Production"
set "DataProtection__KeysPath=%CINEMA_ROOT%\work\keys-sqlite"
echo Сайт: https://danila.inteb.pro
powershell.exe -NoProfile -Command "$root=$env:CINEMA_ROOT; $app=$null; $proxy=$null; try { $app=Start-Process -FilePath ($root+'\work\sdk\dotnet.exe') -ArgumentList @(($root+'\work\domain-build\Cinema.dll'),'--urls','http://127.0.0.1:5157') -WorkingDirectory ($root+'\outputs\Cinema') -WindowStyle Hidden -PassThru; $proxy=Start-Process -FilePath ($root+'\work\caddy.exe') -ArgumentList @('run','--config',($root+'\work\domain\Caddyfile'),'--adapter','caddyfile') -WorkingDirectory $root -WindowStyle Hidden -PassThru; Read-Host 'Нажмите Enter для остановки сайта' } finally { if($proxy -and !$proxy.HasExited){$proxy.Kill()}; if($app -and !$app.HasExited){$app.Kill()} }"
