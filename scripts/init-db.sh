#!/bin/bash

echo "Waiting for SQL Server to start..."
sleep 30

echo "Running database initialization scripts..."

for script in /docker-entrypoint-initdb.d/*.sql; do
    echo "Executing $script..."
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "TelecomBilling123!" -i "$script"
done

echo "Database initialization completed!"
