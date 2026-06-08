FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY DemoDockerAPI2.csproj NuGet.Config ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} dotnet DemoDockerAPI2.dll"]
