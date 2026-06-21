FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/CraftSwap.API/CraftSwap.API.csproj CraftSwap.API/
RUN dotnet restore CraftSwap.API/CraftSwap.API.csproj
COPY src/CraftSwap.API/ CraftSwap.API/
WORKDIR /src/CraftSwap.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8091
ENV ASPNETCORE_URLS=http://+:8091
ENTRYPOINT ["dotnet", "CraftSwap.dll"]
