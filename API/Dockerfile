# base image, add a reference to this with AS
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

#this sets the directory that the container starts in
WORKDIR /app

#copy csproj to workdir and restore as  distinct layers
COPY *.csproj ./
#restores all of our dependencies that are listed inside csproj 
#into our container, working directory
RUN dotnet restore

#copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

#build runtime image, just need the small runtime to run image and not the full sdk
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "API.dll" ]


