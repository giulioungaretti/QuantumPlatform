FROM microsoft/dotnet:2.2-sdk AS base

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

WORKDIR /src/Silo.Host
RUN dotnet build -c Release -o /app
RUN dotnet publish -c Release -o /app

FROM microsoft/dotnet:2:2-aspnetcore-runtime as final
WORKDIR /app
COPY --from=base /app .
ENTRYPOINT ["dotnet", "SiloHost.dll"]
