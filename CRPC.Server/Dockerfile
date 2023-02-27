#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CRPC.Server/CRPC.Server.csproj", "CRPC.Server/"]
COPY ["CRPC.Common/CRPC.Common.csproj", "CRPC.Common/"]
RUN dotnet restore "CRPC.Server/CRPC.Server.csproj"
COPY . .
WORKDIR "/src/CRPC.Server"
RUN dotnet build "CRPC.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CRPC.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CRPC.Server.dll"]