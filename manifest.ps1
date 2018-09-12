if (!(Test-Path ~/.docker)) { mkdir ~/.docker }
'{ "experimental": "enabled" }' | Out-File ~/.docker/config.json -Encoding Ascii

dir ~/.docker
dir ~/.docker/config.json
Get-Content ~/.docker/config.json | ForEach-Object {Write-Output $_}

docker --config ~/.docker manifest create tobiasfenster/plannerexandimport:latest tobiasfenster/plannerexandimport:linux tobiasfenster/plannerexandimport:windows
docker manifest push tobiasfenster/plannerexandimport:latest 