#!/bin/sh

echo "Applying database migrations..."
dotnet ef database update --connection "Server=mysql;Port=3306;Database=blogdb;Uid=root;Pwd=P9l12&zD8A82G!(0z;" --startup-project /app/BloggerWebApi.csproj

echo "Starting the API..."
dotnet BloggerWebApi.dll
