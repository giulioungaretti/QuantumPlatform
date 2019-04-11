FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build


WORKDIR /src
COPY *.sln ./
COPY Server/Server.fsproj Server/
COPY Grains/Grains.fsproj Grains/
COPY Interfaces/Interfaces.fsproj Interfaces/
COPY Silo.Host/SiloHost.fsproj Silo.Host/
COPY Client/src/Main/Main.fsproj Client/src/Main/
COPY Client/src/Renderer/Renderer.fsproj Client/src/Renderer/
COPY Shared/Shared.fsproj Shared/
RUN dotnet restore
COPY . .

WORKDIR /src/Server/
RUN dotnet build -c Release -o /app
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Server.dll"]