FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["Aniflix_WebAPI/Aniflix_WebAPI.csproj", "Aniflix_WebAPI/"]
RUN dotnet restore "Aniflix_WebAPI/Aniflix_WebAPI.csproj"
COPY . .
WORKDIR "/src/Aniflix_WebAPI"
RUN dotnet build "Aniflix_WebAPI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Aniflix_WebAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Aniflix_WebAPI.dll"]