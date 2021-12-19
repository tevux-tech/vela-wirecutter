#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

# Installing general utils
RUN apt-get update
RUN apt-get install -y nano
RUN apt-get install -y inetutils-ping 
RUN apt-get install -y fonts-symbola
RUN apt-get install -y fonts-dejavu

# Installing Tinkerforge related stuff
RUN apt-get install -y wget gnupg lsb-base lsb-release
RUN wget https://download.tinkerforge.com/apt/debian/archive.key -q -O - | apt-key add -
RUN sh -c "echo 'deb https://download.tinkerforge.com/apt/debian $(lsb_release -cs) main' > /etc/apt/sources.list.d/tinkerforge.list"
RUN apt-get update
RUN apt-get install -y brickd

# Cleaning up
RUN rm -rf /var/lib/apt/lists/*

# Building
FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src
COPY . .
WORKDIR "/src/Vela"
RUN dotnet build "Vela.csproj" -c Release -o /app/build

# Publishing
FROM build AS publish
RUN dotnet publish "Vela.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Let's go boys!
ENTRYPOINT ["dotnet", "Vela.dll"]