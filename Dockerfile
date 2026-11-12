# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies first (optimizes layer caching)
COPY ["Agridron.Backend.API/Agridron.Backend.API.csproj", "Agridron.Backend.API/"]
RUN dotnet restore "Agridron.Backend.API/Agridron.Backend.API.csproj"

# Copy all source code
COPY . .

# Publish release package
WORKDIR "/src/Agridron.Backend.API"
RUN dotnet publish "Agridron.Backend.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Final Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Standard cloud container port configuration (compatible with Render, Cloud Run, Azure)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Agridron.Backend.API.dll"]
