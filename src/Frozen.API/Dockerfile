FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Frozen.Domain/Frozen.Domain.csproj", "src/Frozen.Domain/"]
COPY ["src/Frozen.Application/Frozen.Application.csproj", "src/Frozen.Application/"]
COPY ["src/Frozen.Infrastructure/Frozen.Infrastructure.csproj", "src/Frozen.Infrastructure/"]
COPY ["src/Frozen.API/Frozen.API.csproj", "src/Frozen.API/"]
RUN dotnet restore "src/Frozen.API/Frozen.API.csproj"

COPY . .
WORKDIR /src/src/Frozen.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Frozen.API.dll"]