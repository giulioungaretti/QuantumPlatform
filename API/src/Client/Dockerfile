FROM microsoft/dotnet:2.1-sdk AS base
WORKDIR /app
# TODO: expose the correct port 
EXPOSE 80

WORKDIR /src
COPY *.sln ./
COPY Client/Client.fsproj Client/
COPY Grains/Grains.fsproj Grains/
COPY Interfaces/Interfaces.fsproj Interfaces/
COPY Silo.Host/SiloHost.fsproj Silo.Host/
RUN dotnet restore
COPY . .
WORKDIR /src/Client/
RUN dotnet build -c Release -o /app
RUN dotnet publish -c Release -o /app

FROM microsoft/dotnet:2.1-runtime as final

WORKDIR /app
COPY --from=base /app .
ENTRYPOINT ["dotnet", "Client.dll"]