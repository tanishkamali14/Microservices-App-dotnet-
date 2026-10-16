FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
# Run as the non-root user that ships with the .NET 8+ images.
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Restore first so the package layer is cached until a project file changes.
COPY ["global.json", "Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["src/Shopping.WebApp/Shopping.WebApp.csproj", "src/Shopping.WebApp/"]
COPY ["src/Shop.ServiceDefaults/Shop.ServiceDefaults.csproj", "src/Shop.ServiceDefaults/"]
RUN dotnet restore "src/Shopping.WebApp/Shopping.WebApp.csproj"
COPY . .
WORKDIR "/src/src/Shopping.WebApp"
RUN dotnet publish "Shopping.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Shopping.WebApp.dll"]
