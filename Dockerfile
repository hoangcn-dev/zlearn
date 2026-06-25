# build to dll
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . . 
RUN dotnet restore 
RUN dotnet publish -c Release -o out

# prepare to run dll
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80

# Set timezone to Asia/Ho_Chi_Minh (UTC+7)
ENV TZ=Asia/Ho_Chi_Minh
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# Install postgresql-client to use pg_dump
RUN apt update && apt install -y wget gnupg
RUN echo "deb http://apt.postgresql.org/pub/repos/apt/ bullseye-pgdg main" > /etc/apt/sources.list.d/pgdg.list \
    && wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | apt-key add - \
    && apt update \
    && apt install -y postgresql-client-16

COPY --from=build /app/out .
ENTRYPOINT [ "dotnet", "ZLearn.Web.dll" ]