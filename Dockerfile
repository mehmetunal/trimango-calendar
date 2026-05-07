FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/TrimangoCalendar.sln ./TrimangoCalendar.sln
COPY backend/TrimangoCalendar.Core/TrimangoCalendar.Core.csproj backend/TrimangoCalendar.Core/
COPY backend/TrimangoCalendar.Data/TrimangoCalendar.Data.csproj backend/TrimangoCalendar.Data/
COPY backend/TrimangoCalendar.Infrastructure/TrimangoCalendar.Infrastructure.csproj backend/TrimangoCalendar.Infrastructure/
COPY backend/TrimangoCalendar.Shared/TrimangoCalendar.Shared.csproj backend/TrimangoCalendar.Shared/
COPY backend/TrimangoCalendar.API/TrimangoCalendar.API.csproj backend/TrimangoCalendar.API/
RUN dotnet restore "backend/TrimangoCalendar.API/TrimangoCalendar.API.csproj"

# Build and Publish
COPY backend/. ./backend/
WORKDIR /src/backend/TrimangoCalendar.API
RUN dotnet publish "TrimangoCalendar.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "TrimangoCalendar.API.dll"]