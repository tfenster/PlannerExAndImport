ARG SDK
ARG RUNTIME
FROM microsoft/dotnet:$SDK AS build
WORKDIR /app

COPY PlannerExAndImport.csproj .
RUN dotnet restore

COPY *.cs ./
COPY JSON/. ./JSON
RUN dotnet build
RUN dotnet publish -c Release

FROM microsoft/dotnet:$RUNTIME AS runtime
WORKDIR /app
COPY --from=build /app/bin/Release/netcoreapp2.1/publish/. ./
