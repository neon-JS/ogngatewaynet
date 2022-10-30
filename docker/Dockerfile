FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /source

COPY OgnGateway.sln .
COPY OgnGateway/ ./OgnGateway/
COPY WebsocketGateway/ ./WebsocketGateway/

# Remove possibly existing build artifacts
RUN rm -rf */bin */obj *.json

RUN dotnet publish -c release -o /app /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app ./
COPY WebsocketGateway/appsettings.json /app/

ENTRYPOINT ["dotnet", "WebsocketGateway.dll"]