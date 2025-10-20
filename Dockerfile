# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj để cache restore
COPY Veggies_EXE201/Veggies_EXE201.csproj Veggies_EXE201/
RUN dotnet restore Veggies_EXE201/Veggies_EXE201.csproj

# copy toàn bộ source và publish
COPY . .
RUN dotnet publish Veggies_EXE201/Veggies_EXE201.csproj -c Release -o /app

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Render cung cấp PORT → lắng nghe toàn cục
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app ./

ENTRYPOINT ["dotnet", "Veggies_EXE201.dll"]
