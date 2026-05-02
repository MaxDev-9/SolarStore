FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create persistent data directory
RUN mkdir -p /data

COPY --from=build /app/publish .

# Railway injects $PORT; ASP.NET listens on it
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV DB_PATH=/data/solar.db

EXPOSE 8080

ENTRYPOINT ["dotnet", "SolarStore.dll"]
