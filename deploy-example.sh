#!/bin/bash
# Переменные окружения
export VERSION=1.0.0
export JWT_SECRET_KEY="your_production_secret_key"
# Остановка существующих контейнеров
docker-compose down
# Сборка новых образов
docker-compose build
# Запуск в production режиме
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
# Проверка статуса
docker-compose ps
echo "Deployment completed successfully!"