#!/usr/bin/env nu

dotnet tool restore
dotnet fable ../Farss.Client/src/ --noCache 
cd ../Farss.Client
yarn
yarn run build --outDir ../publish/wwwroot --base ""
dotnet publish ..\Farss.Server\ -c release -o ../publish
cd ..
docker login -u $CI_REGISTRY_USER -p $CI_REGISTRY_PASSWORD $CI_REGISTRY
docker build -t registry.gitlab.com/chrisjansson/farss .
docker push registry.gitlab.com/chrisjansson/farss
