# ---- Build Aşaması ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasını kopyala ve restore et
COPY ["EminAutoPrime/EminAutoPrime.csproj", "EminAutoPrime/"]
RUN dotnet restore "EminAutoPrime/EminAutoPrime.csproj"

# Geri kalan tüm dosyaları kopyala ve publish et
COPY . .
WORKDIR "/src/EminAutoPrime"
RUN dotnet publish "EminAutoPrime.csproj" -c Release -o /app/publish

# ---- Çalıştırma Aşaması ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EminAutoPrime.dll"]
