FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . /src/backend
WORKDIR /src/backend/TrimangoCalendar.API
RUN dotnet restore "TrimangoCalendar.API.csproj"

# Build + Publish
COPY . .
WORKDIR "/src/backend/TrimangoCalendar.API"
RUN dotnet publish "TrimangoCalendar.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "TrimangoCalendar.API.dll"]
