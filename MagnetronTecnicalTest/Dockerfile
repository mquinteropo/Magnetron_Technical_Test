# Usar la imagen base oficial de ASP.NET Core Runtime 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Usar la imagen SDK para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar el archivo del proyecto y restaurar dependencias
COPY ["MagnetronTecnicalTest/MagnetronTecnicalTest.csproj", "MagnetronTecnicalTest/"]
RUN dotnet restore "MagnetronTecnicalTest/MagnetronTecnicalTest.csproj"

# Copiar todo el código fuente y construir
COPY . .
WORKDIR "/src/MagnetronTecnicalTest"
RUN dotnet build "MagnetronTecnicalTest.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar la aplicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MagnetronTecnicalTest.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear usuario no-root para seguridad
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "MagnetronTecnicalTest.dll"]