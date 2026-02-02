# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EventBus/EventBus.csproj", "EventBus/"]
COPY ["EventBus.Kafka/EventBus.Kafka.csproj", "EventBus.Kafka/"]
COPY ["ESsample.Banking.Shared/ESsample.Banking.Shared.csproj", "ESsample.Banking.Shared/"]
COPY ["Projections.Banking/Projections.Banking.csproj", "Projections.Banking/"]
COPY ["Projections.Banking.Postgres/Projections.Banking.Postgres.csproj", "Projections.Banking.Postgres/"]
COPY ["Projections.Banking.Consumer/Projections.Banking.Consumer.csproj", "Projections.Banking.Consumer/"]
RUN dotnet restore "./Projections.Banking.Consumer/Projections.Banking.Consumer.csproj"
COPY . .
WORKDIR "/src/Projections.Banking.Consumer"
RUN dotnet build "./Projections.Banking.Consumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Projections.Banking.Consumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Projections.Banking.Consumer.dll"]
