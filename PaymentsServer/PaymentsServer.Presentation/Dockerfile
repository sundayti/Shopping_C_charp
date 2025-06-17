FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PaymentsServer.Presentation/PaymentsServer.Presentation.csproj", "PaymentsServer.Presentation/"]
COPY ["PaymentsServer.Infrastructure/PaymentsServer.Infrastructure.csproj", "PaymentsServer.Infrastructure/"]
COPY ["PaymentsServer.Domain/PaymentsServer.Domain.csproj", "PaymentsServer.Domain/"]
COPY ["PaymentsServer.Application/PaymentsServer.Application.csproj", "PaymentsServer.Application/"]
RUN dotnet restore "PaymentsServer.Presentation/PaymentsServer.Presentation.csproj"
COPY . .
WORKDIR "/src/PaymentsServer.Presentation"
RUN dotnet build "PaymentsServer.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PaymentsServer.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentsServer.Presentation.dll"]
