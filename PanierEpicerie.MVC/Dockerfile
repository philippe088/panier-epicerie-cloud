FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PanierEpicerie.MVC.csproj", "./"]
RUN dotnet restore "PanierEpicerie.MVC.csproj"

COPY . .

RUN dotnet build "PanierEpicerie.MVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PanierEpicerie.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PanierEpicerie.MVC.dll"]
