#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app
EXPOSE 8081

COPY *.csproj .

RUN dotnet restore "./DeYasnoTelegramBot.csproj"

COPY . ./
FROM base AS publish
RUN dotnet publish --no-restore -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=base /app/out .
ENTRYPOINT ["dotnet", "DeYasnoTelegramBot.dll"]