# build to dll
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY . . 
RUN dotnet restore 
RUN dotnet publish -c Release -o out

# prepare to run dll
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/out .
ENTRYPOINT [ "dotnet", "ZLearn.Web.dll" ]