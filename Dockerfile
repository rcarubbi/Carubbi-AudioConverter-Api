FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY Carubbi.AudioConverter.Api ./Carubbi.AudioConverter.Api

RUN dotnet publish Carubbi.AudioConverter.Api/Carubbi.AudioConverter.Api.csproj \
    -c Release -o /app --nologo

FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update \
    && apt-get install -y --no-install-recommends opus-tools \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Carubbi.AudioConverter.Api.dll"]
