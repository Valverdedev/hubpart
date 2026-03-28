FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AutoPartsHub.API/AutoPartsHub.API.csproj", "src/AutoPartsHub.API/"]
COPY ["src/AutoPartsHub.Application/AutoPartsHub.Application.csproj", "src/AutoPartsHub.Application/"]
COPY ["src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj", "src/AutoPartsHub.Domain/"]
COPY ["src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj", "src/AutoPartsHub.Infra/"]
RUN dotnet restore "src/AutoPartsHub.API/AutoPartsHub.API.csproj"
COPY . .
RUN dotnet build "src/AutoPartsHub.API/AutoPartsHub.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/AutoPartsHub.API/AutoPartsHub.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoPartsHub.API.dll"]
