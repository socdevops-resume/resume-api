# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore first (better layer caching)
COPY *.sln ./
COPY **/*.csproj ./
# If you have multiple projects, replace with explicit COPYs or `COPY . .` and a solution restore
RUN dotnet restore

# Copy everything and publish
COPY . .
RUN dotnet publish -c Release -o /app/out

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Set listening port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# (Optional) Common env vars your app may use
# ENV MongoDBSettings__ConnectionString=...
# ENV Jwt__Issuer=...
# ENV Jwt__Audience=...
# ENV Jwt__Key=...
# ENV Llm__Provider=...

COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "YourBackendAssembly.dll"]
