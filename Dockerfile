FROM microsoft/dotnet:2.1-sdk-nanoserver-sac2016 AS build
WORKDIR /app

COPY PlannerExAndImport.csproj .
RUN dotnet restore

COPY *.cs ./
COPY JSON/. ./JSON
RUN dotnet build
RUN dotnet publish -c Release -r win10-x64


FROM microsoft/dotnet:2.1-runtime-nanoserver-sac2016 AS runtime
WORKDIR /app
COPY --from=build /app/bin/Release/netcoreapp2.1/win10-x64/publish/. ./

CMD cmd