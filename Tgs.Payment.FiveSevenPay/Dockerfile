#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.Payment.FiveSevenPay/Tgs.Payment.FiveSevenPay.csproj", "Tgs.Payment.FiveSevenPay/"]
COPY ["FiveSevenPay.Core/FiveSevenPay.Core.csproj", "FiveSevenPay.Core/"]
COPY ["FiveSevenPay.Client/FiveSevenPay.Client.csproj", "FiveSevenPay.Client/"]
RUN dotnet restore "Tgs.Payment.FiveSevenPay/Tgs.Payment.FiveSevenPay.csproj"
COPY . .
WORKDIR "/src/Tgs.Payment.FiveSevenPay"
RUN dotnet build "Tgs.Payment.FiveSevenPay.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.Payment.FiveSevenPay.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Payment.FiveSevenPay.dll"]