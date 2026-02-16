# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:10 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Estágio de Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "todo-list.dll"]