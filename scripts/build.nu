#!/usr/bin/env nu

dotnet fable ../Farss.Client/src/ --noCache 
cd ../Farss.Client
yarn run build --outDir ../publish/wwwroot --base ""
dotnet publish ..\Farss.Server\ -c release -o ../publish
cd ..
docker build -t registry.gitlab.com/chrisjansson/farss .
docker push registry.gitlab.com/chrisjansson/farss
