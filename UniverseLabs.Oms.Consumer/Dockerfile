FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UniverseLabs.Oms.Consumer/UniverseLabs.Oms.Consumer.csproj", "UniverseLabs.Oms.Consumer/"]
COPY ["UniverseLabs.Common/UniverseLabs.Common.csproj", "UniverseLabs.Common/"]
COPY ["UniverseLabs.Messages/UniverseLabs.Messages.csproj", "UniverseLabs.Messages/"]
COPY ["Models/Models.csproj", "Models/"]
RUN dotnet restore "UniverseLabs.Oms.Consumer/UniverseLabs.Oms.Consumer.csproj"
COPY . .
WORKDIR "/src/UniverseLabs.Oms.Consumer"
RUN dotnet build "UniverseLabs.Oms.Consumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UniverseLabs.Oms.Consumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UniverseLabs.Oms.Consumer.dll"]
