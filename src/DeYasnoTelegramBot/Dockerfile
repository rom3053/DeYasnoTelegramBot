#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["./DeYasnoTelegramBot/DeYasnoTelegramBot.csproj", "DeYasnoTelegramBot/"]
RUN dotnet restore "./src/DeYasnoTelegramBot/DeYasnoTelegramBot.csproj"
COPY . .
WORKDIR "/src/src/DeYasnoTelegramBot"
RUN dotnet build "./DeYasnoTelegramBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./DeYasnoTelegramBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DeYasnoTelegramBot.dll"]