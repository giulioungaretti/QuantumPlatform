FROM microsoft/dotnet:2.2-sdk AS base
WORKDIR /src
COPY *.sln ./
COPY Client/Client.fsproj Client/
COPY Grains/Grains.fsproj Grains/
COPY Interfaces/Interfaces.fsproj Interfaces/
COPY Silo.Host/SiloHost.fsproj Silo.Host/
RUN dotnet restore
COPY . .
WORKDIR /src/Silo.Host
RUN dotnet build -c Release -o /app
RUN dotnet publish -c Release -o /app

FROM microsoft/dotnet:2.2-aspnet as final
WORKDIR /app
COPY --from=base /app .
ENTRYPOINT ["dotnet", "SiloHost.dll"]
