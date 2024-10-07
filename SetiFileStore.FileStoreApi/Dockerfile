FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SetiFileStore.FileStoreApi/SetiFileStore.FileStoreApi.csproj", "SetiFileStore.FileStoreApi/"]
COPY ["SetiFileStore.Domain/SetiFileStore.Domain.csproj", "SetiFileStore.Domain/"]
COPY ["SetiFileStore.Infrastructure/SetiFileStore.Infrastructure.csproj", "SetiFileStore.Infrastructure/"]
RUN dotnet restore "SetiFileStore.FileStoreApi/SetiFileStore.FileStoreApi.csproj"
COPY . .
WORKDIR "/src/SetiFileStore.FileStoreApi"
RUN dotnet build "SetiFileStore.FileStoreApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SetiFileStore.FileStoreApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SetiFileStore.FileStoreApi.dll"]
