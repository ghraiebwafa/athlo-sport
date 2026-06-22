FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Athlo.Shared/", "Athlo.Shared/"]
COPY ["DataAccess/", "DataAccess/"]
COPY ["Services/Athlo.ManagementService/", "Services/Athlo.ManagementService/"]
RUN dotnet restore "Services/Athlo.ManagementService/Athlo.ManagementService.csproj"
RUN dotnet publish "Services/Athlo.ManagementService/Athlo.ManagementService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
RUN adduser --disabled-password --gecos '' appuser
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
USER appuser
ENTRYPOINT ["dotnet", "Athlo.ManagementService.dll"]
