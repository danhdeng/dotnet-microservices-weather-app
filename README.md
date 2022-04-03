…or create a new repository on the command line

echo "# dotnet-microservices-weather-app" >> README.md

git init

git add README.md

git commit -m "first commit"

git branch -M main

git remote add origin https://github.com/danhdeng/dotnet-microservices-weather-app.git

git push -u origin main

…or push an existing repository from the command line

git remote add origin https://github.com/danhdeng/dotnet-microservices-weather-app.git

git branch -M main

git push -u origin main

# create a new solution folder and solution file

dotnet new sln -o Microservices-Weather

# Create three web api services

dotnet new webapi -o CloudWeather.Precipitation
dotnet new webapi -o CloudWeather.Temperature
dotnet new webapi -o CloudWeather.Report

#Create a console app to load data into database

dotnet new console -o CloudWeather.DataLoader

# add all projects in the sub-directory running in powershell

dotnet sln Microservices-Weather.sln add (ls -r \*_/_.csproj)
