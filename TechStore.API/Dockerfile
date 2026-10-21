FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TechStore.API/TechStore.API.csproj TechStore.API/
RUN dotnet restore TechStore.API/TechStore.API.csproj

COPY TechStore.API/ TechStore.API/

RUN dotnet publish TechStore.API/TechStore.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ConnectionStrings__DefaultConnection="Data Source=/data/TechStore.db"

RUN mkdir -p /data && chown app:app /data

COPY --from=build /app/publish .

USER app

EXPOSE 8080

ENTRYPOINT ["dotnet", "TechStore.API.dll"]