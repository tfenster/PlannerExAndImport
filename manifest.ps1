if (!(Test-Path ~/.docker)) { mkdir ~/.docker }
'{ "experimental": "enabled" }' | Out-File ~/.docker/config.json -Encoding Ascii
docker manifest create tobiasfenster/plannerexandimport:latest  tobiasfenster/plannerexandimport:linux tobiasfenster/plannerexandimport:windows
docker manifest push tobiasfenster/plannerexandimport:latest 