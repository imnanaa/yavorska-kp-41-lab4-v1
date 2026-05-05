FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["Worker.Departments/Worker.Departments.csproj", "Worker.Departments/"]
RUN dotnet restore "Worker.Departments/Worker.Departments.csproj"

COPY . .
WORKDIR "/src/Worker.Departments"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Worker.Departments.dll"]
