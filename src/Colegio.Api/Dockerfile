FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Colegio.Api/Colegio.Api.csproj", "Colegio.Api/"]
COPY ["Colegio.Domain/Colegio.Domain.csproj", "Colegio.Domain/"]
COPY ["Colegio.Infrastructure/Colegio.Infrastructure.csproj", "Colegio.Infrastructure/"]
RUN dotnet restore "Colegio.Api/Colegio.Api.csproj"
COPY . .
WORKDIR "/src/Colegio.Api"
RUN dotnet build "Colegio.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Colegio.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Colegio.Api.dll"]
