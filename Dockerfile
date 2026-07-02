# build to dll
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
# Copy csproj files
COPY Zlearn.V2.Domain/Zlearn.V2.Domain.csproj Zlearn.V2.Domain/
COPY Zlearn.V2.Application/Zlearn.V2.Application.csproj Zlearn.V2.Application/
COPY Zlearn.V2.Infas/Zlearn.V2.Infas.csproj Zlearn.V2.Infas/
COPY ZLearn.Web/ZLearn.Web.csproj ZLearn.Web/

# Restore dependencies
RUN dotnet restore ZLearn.Web/ZLearn.Web.csproj

# Copy source code
COPY Zlearn.V2.Domain/ Zlearn.V2.Domain/
COPY Zlearn.V2.Application/ Zlearn.V2.Application/
COPY Zlearn.V2.Infas/ Zlearn.V2.Infas/
COPY ZLearn.Web/ ZLearn.Web/

RUN dotnet publish ZLearn.Web/ZLearn.Web.csproj -c Release -o out

# prepare to run dll
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
EXPOSE 8080

# Set timezone to Asia/Ho_Chi_Minh (UTC+7)
ENV TZ=Asia/Ho_Chi_Minh
RUN apk add --no-cache tzdata \
    && ln -snf /usr/share/zoneinfo/$TZ /etc/localtime \
    && echo $TZ > /etc/timezone

# Install postgresql-client to use pg_dump
RUN apk add --no-cache postgresql-client

COPY --from=build /app/out .
ENTRYPOINT [ "dotnet", "ZLearn.Web.dll" ]