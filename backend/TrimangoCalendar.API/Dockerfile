FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore
COPY ["TrimangoCalendar.API/TrimangoCalendar.API.csproj", "TrimangoCalendar.API/"]
COPY ["TrimangoCalendar.Core/TrimangoCalendar.Core.csproj", "TrimangoCalendar.Core/"]
COPY ["TrimangoCalendar.Data/TrimangoCalendar.Data.csproj", "TrimangoCalendar.Data/"]
COPY ["TrimangoCalendar.Infrastructure/TrimangoCalendar.Infrastructure.csproj", "TrimangoCalendar.Infrastructure/"]
COPY ["TrimangoCalendar.Shared/TrimangoCalendar.Shared.csproj", "TrimangoCalendar.Shared/"]
RUN dotnet restore "TrimangoCalendar.API/TrimangoCalendar.API.csproj"

# Build + Publish
COPY . .
WORKDIR "/src/TrimangoCalendar.API"
RUN dotnet publish "TrimangoCalendar.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "TrimangoCalendar.API.dll"]
