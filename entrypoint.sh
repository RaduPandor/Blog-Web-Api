echo "Applying database migrations..."
dotnet ef database update --connection "Server=mysql;Port=3306;Database=mydb;Uid=root;Pwd=rootpassword;" --startup-project /app/BloggerWebApi.csproj

echo "Starting the API..."
dotnet BloggerWebApi.dll
