# SmartHome
1.скачать Docker и docker-compose \
2.скачать dotnet-ef для миграций \
3.скачать Prosys simulation server version 5.6.0-6 


```bash
cd ./SmartHome
docker-compose -f ./docker-compose.infra.yml
docker-compose -f ./docker-compose.monitoring.yml
#обновляем миграции
dotnet ef database update -s SmartHome.API -p SmartHome.Infra
```
4. заходите в БД например через psql 
```bash
docker exec -it timescaledb psql -U postgres -d industrial -c "SELECT create_hypertable('\"SensorTelemetry\"', 'Time', chunk_time_interval => INTERVAL '1 days', if_not_exists => TRUE, create_default_indexes => FALSE);"

```

вывод должен быть таким:
```bash
      create_hypertable       
------------------------------
 (1,public,SensorTelemetry,t)
(1 row)
```
5. вводите команду создания гипертаблицы с вашими настройками
```bash
SELECT create_hypertable(
    '"SensorTelemetry"',        -- Имя таблицы с учетом регистра
    'Time',                     -- Имя колонки с временной меткой
    chunk_time_interval => INTERVAL '1 days', -- Оптимальный интервал чанка
    if_not_exists => TRUE,      -- Избегаем ошибки, если таблица уже гипертаблица
    create_default_indexes => FALSE -- Не создаем лишние индексы, т.к. вы управляете ими через EF
);
```

⚙️ Дополнительная рекомендация: Управление через миграцию

Для полной автоматизации вы можете создать новую, пустую миграцию и добавить эту команду в её Up метод с помощью migrationBuilder.Sql().
```csharp

// В методе Up новой миграции
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(
        @"SELECT create_hypertable(
            '""SensorTelemetry""',
            'Time',
            chunk_time_interval => INTERVAL '7 days',
            if_not_exists => TRUE,
            create_default_indexes => FALSE
        );"
    );
}
```


6. БЕНЧМАРК производительности приложения
запуск осуществляется таким образом:
```
cd SmartHome.Testscd 
dotnet run -c Release
```

7.ЗАПУСК ОСНОВНОГО ПРОЕКТА в окружении разработчика
```
cd SmartHome.API
dotnet run
```
данные в базе 
