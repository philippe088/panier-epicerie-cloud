FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Cloud.ExamenFinal.MVC.csproj", "./"]
RUN dotnet restore "Cloud.ExamenFinal.MVC.csproj"

COPY . .

RUN dotnet build "Cloud.ExamenFinal.MVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cloud.ExamenFinal.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Cloud.ExamenFinal.MVC.dll"]
