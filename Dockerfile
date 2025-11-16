# ===== Build & publish stage =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj + restore
COPY ["src/CVGenerator.API/CVGenerator.API.csproj", "CVGenerator.API/"]
RUN dotnet restore "CVGenerator.API/CVGenerator.API.csproj"

# Copy the rest and publish
COPY ["src/CVGenerator.API/", "CVGenerator.API/"]
RUN dotnet publish "CVGenerator.API/CVGenerator.API.csproj" -c Release -o /app/publish

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "CVGenerator.API.dll"]
