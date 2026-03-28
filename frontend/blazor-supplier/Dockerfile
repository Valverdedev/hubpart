FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj", "frontend/blazor-supplier/"]
COPY ["src/AutoPartsHub.Application/AutoPartsHub.Application.csproj", "src/AutoPartsHub.Application/"]
COPY ["src/AutoPartsHub.Domain/AutoPartsHub.Domain.csproj", "src/AutoPartsHub.Domain/"]
COPY ["src/AutoPartsHub.Infra/AutoPartsHub.Infra.csproj", "src/AutoPartsHub.Infra/"]
RUN dotnet restore "frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj"
COPY . .
RUN dotnet build "frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "frontend/blazor-supplier/AutoPartsHub.Blazor.Supplier.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoPartsHub.Blazor.Supplier.dll"]
