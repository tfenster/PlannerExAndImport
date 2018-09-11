ARG SDK=2.1-sdk
FROM microsoft/dotnet:$BUILD_IMAGE AS build
WORKDIR /app

COPY PlannerExAndImport.csproj .
RUN dotnet restore

COPY *.cs ./
COPY JSON/. ./JSON
RUN dotnet build
ARG RID=win10-x64
RUN dotnet publish -c Release -r $RELEASE_TYPE

ARG RUNTIME=2.1-runtime
FROM microsoft/dotnet:$RUNTIME AS runtime
WORKDIR /app
COPY --from=build /app/bin/Release/netcoreapp2.1/$RID/publish/. ./
