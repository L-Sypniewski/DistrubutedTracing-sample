FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base
WORKDIR /app

RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /src
COPY ["WebApiFirst/WebApiFirst.csproj", "WebApiFirst/"]
RUN dotnet restore "WebApiFirst/WebApiFirst.csproj"
COPY . .
WORKDIR "/src/WebApiFirst"
RUN dotnet build "WebApiFirst.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApiFirst.csproj" -c Release -r linux-x64 -p:PublishTrimmed=true -o /app/publish

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "./WebApiFirst.dll"]
