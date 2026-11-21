FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MyHttpServer/MyHttpServer.csproj", "MyHttpServer/"]
COPY ["MiniHttpServer.Framework/MiniHttpServer.Framework.csproj", "MiniHttpServer.Framework/"]
COPY ["MiniHttpServer.ORM/MiniHttpServer.ORM.csproj", "MiniHttpServer.ORM/"]
COPY ["MiniHttpServer.HtmlTemplating/MiniHttpServer.HtmlTemplating.csproj", "MiniHttpServer.HtmlTemplating/"]

RUN dotnet restore "MyHttpServer/MyHttpServer.csproj"

COPY . .
WORKDIR "/src/MyHttpServer"
RUN dotnet publish "MyHttpServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

COPY MyHttpServer/settings.Docker.json /app/settings.json

EXPOSE 8888

ENTRYPOINT ["dotnet", "MyHttpServer.dll"]