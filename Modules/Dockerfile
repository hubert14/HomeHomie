FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

ARG module_name

COPY ./Core ./Core
COPY ./Modules/HomeHomie.${module_name}Module ./Modules/HomeHomie.${module_name}Module

RUN dotnet publish ./Modules/HomeHomie.${module_name}Module/HomeHomie.${module_name}Module.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=build-env /App/out .