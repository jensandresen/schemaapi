FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine

WORKDIR /app
COPY .output/app/ /app/

ENTRYPOINT [ "dotnet", "SchemaApi.App.dll" ]