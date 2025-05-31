FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости
COPY AsiaGuides.csproj AsiaGuides/
RUN dotnet restore AsiaGuides.csproj

# Копируем остальные файлы и публикуем
COPY . .
WORKDIR /src/AsiaGuides
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AsiaGuides.dll"]
