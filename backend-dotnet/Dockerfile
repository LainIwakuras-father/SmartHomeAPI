# Многостадийная сборка для оптимизации размера образа
FROM mcr.microsoft.com/dotnet/aspnet:8.0  AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Копирование файлов проектов
COPY ["SmartHome.API/SmartHome.API.csproj","SmartHome.API/"] 
COPY ["SmartHome.Application/SmartHome.Application.csproj","SmartHome.Application/"] 
COPY ["SmartHome.Core/SmartHome.Core.csproj","SmartHome.Core/"] 
COPY ["SmartHome.gRPCServer/SmartHome.gRPCServer.csproj","SmartHome.gRPCServer/"] 
COPY ["SmartHome.Infra/SmartHome.Infra.csproj","SmartHome.Infra/"] 

# Восстановление зависимостей
RUN dotnet restore "SmartHome.API/SmartHome.API.csproj"

# Копирование исходного кода
COPY . .
# Сборка проекта
WORKDIR "/src/SmartHome.API"
RUN dotnet build "SmartHome.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartHome.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 50051
COPY --from=publish /app/publish .

# Создание пользователя для безопасности (не root)
RUN adduser \
    --disabled-password \ 
    --home /app \ 
    --gecos '' appuser \ 
    && chown -R appuser /app 
USER appuser 

ENTRYPOINT ["dotnet", "SmartHome.API.dll"]