FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Inventory Management Platform.csproj", "./"]
RUN dotnet restore "Inventory Management Platform.csproj"
COPY . .
RUN dotnet build "./Inventory Management Platform.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Inventory Management Platform.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Development

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
    
ENTRYPOINT ["dotnet", "Inventory Management Platform.dll"]
