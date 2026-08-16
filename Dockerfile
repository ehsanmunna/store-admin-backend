FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Frozen.Domain/*.csproj src/Frozen.Domain/
COPY src/Frozen.Application/*.csproj src/Frozen.Application/
COPY src/Frozen.Infrastructure/*.csproj src/Frozen.Infrastructure/
COPY src/Frozen.API/*.csproj src/Frozen.API/
RUN dotnet restore src/Frozen.API/Frozen.API.csproj

COPY src/ src/
RUN dotnet publish src/Frozen.API/Frozen.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

RUN mkdir -p /app/logs && chown -R app:app /app/logs
USER app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Frozen.API.dll"]
