# create necessary folders
mkdir -p ./src/wwwroot/routes
mkdir -p ./src/wwwroot/requests/data

# build project
dotnet restore ./src
dotnet build ./src
