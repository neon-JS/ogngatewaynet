FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

ARG TARGETARCH

COPY OgnGateway/ ./OgnGateway/
COPY WebsocketGateway/ ./WebsocketGateway/

RUN dotnet restore WebsocketGateway/WebsocketGateway.csproj
RUN dotnet build WebsocketGateway/WebsocketGateway.csproj -c Release -o /app/build -a $TARGETARCH --no-self-contained

FROM build AS publish

ARG TARGETARCH

RUN dotnet publish WebsocketGateway/WebsocketGateway.csproj -c Release -o /app/publish -a $TARGETARCH /p:UseAppHost=false --no-self-contained --no-restore

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

RUN adduser --disabled-password --home /app --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser

ENTRYPOINT ["dotnet", "/app/WebsocketGateway.dll"]