FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["WebPushNotificationEndPointServer/WebPushNotificationEndPointServer.csproj", "WebPushNotificationEndPointServer/"]
RUN dotnet restore "WebPushNotificationEndPointServer/WebPushNotificationEndPointServer.csproj"
COPY . .
WORKDIR "/src/WebPushNotificationEndPointServer"
RUN dotnet build "WebPushNotificationEndPointServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebPushNotificationEndPointServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebPushNotificationEndPointServer.dll"]
