FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["Worker.Prod/Worker.Prod.csproj", "Worker.Prod/"]
RUN dotnet restore "Worker.Prod/Worker.Prod.csproj"

COPY . .
WORKDIR "/src/Worker.Prod"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Worker.Prod.dll"]»
