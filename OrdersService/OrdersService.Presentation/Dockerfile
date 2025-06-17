FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OrdersService.Presentation/OrdersService.Presentation.csproj", "OrdersService.Presentation/"]
COPY ["OrdersService.Application/OrdersService.Application.csproj", "OrdersService.Application/"]
COPY ["OrdersService.Domain/OrdersService.Domain.csproj", "OrdersService.Domain/"]
COPY ["OrdersService.Infrastructure/OrdersService.Infrastructure.csproj", "OrdersService.Infrastructure/"]
RUN dotnet restore "OrdersService.Presentation/OrdersService.Presentation.csproj"
COPY . .
WORKDIR "/src/OrdersService.Presentation"
RUN dotnet build "OrdersService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OrdersService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrdersService.Presentation.dll"]
