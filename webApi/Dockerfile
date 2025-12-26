FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["webApi/webApi.csproj", "webApi/"]
COPY ["Migrations/Migrations.csproj", "Migrations/"]
COPY ["Models/Models.csproj", "Models/"]
COPY ["UniverseLabs.Common/UniverseLabs.Common.csproj", "UniverseLabs.Common/"]
COPY ["UniverseLabs.Messages/UniverseLabs.Messages.csproj", "UniverseLabs.Messages/"]
RUN dotnet restore "webApi/webApi.csproj"
COPY . .
WORKDIR "/src/webApi"
RUN dotnet build "webApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "webApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "webApi.dll"]
