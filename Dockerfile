# Estágio de Build - Usando SDK 10 (Preview) para suportar seu projeto

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

WORKDIR /app



# Copiar tudo e restaurar

COPY . .

RUN dotnet restore



# Publicar o projeto

RUN dotnet publish -c Release -o out



# Estágio de Runtime - Também usando a versão 10

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview

WORKDIR /app

COPY --from=build /app/out .



# IMPORTANTE: O nome aqui deve ser o do seu projeto C# (TodoList)

ENTRYPOINT ["dotnet", "todo-db.dll"]