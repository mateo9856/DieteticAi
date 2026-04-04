FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DieteticAI.UI/DieteticAI.UI.csproj", "DieteticAI.UI/"]
COPY ["DietAI.RabbitServer/DietAI.RabbitServer.csproj", "DietAI.RabbitServer/"]
RUN dotnet restore "DieteticAI.UI/DieteticAI.UI.csproj"
COPY . .
WORKDIR "/src/DieteticAI.UI"
RUN dotnet build "./DieteticAI.UI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DieteticAI.UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DieteticAI.UI.dll"]
