# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ShopFortnite/ShopFortnite.csproj", "ShopFortnite/"]
RUN dotnet restore "ShopFortnite/ShopFortnite.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ShopFortnite"
RUN dotnet build "ShopFortnite.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ShopFortnite.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/shopfortnite.db"

ENTRYPOINT ["dotnet", "ShopFortnite.dll"]
