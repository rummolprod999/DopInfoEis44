FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["DopInfoEis44/DopInfoEis44.fsproj", "DopInfoEis44/"]
RUN dotnet restore "DopInfoEis44/DopInfoEis44.fsproj"
COPY . .
WORKDIR "/src/DopInfoEis44"
RUN dotnet build "DopInfoEis44.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DopInfoEis44.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DopInfoEis44.dll"]
