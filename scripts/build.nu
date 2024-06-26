#!/usr/bin/env nu

dotnet tool restore
dotnet fable ../Farss.Client/src/ --noCache 
cd ../Farss.Client
yarn
yarn run build --outDir ../publish/wwwroot --base ""
dotnet publish ..\Farss.Server\ -c release -o ../publish
cd ../publish
$"(date now)\n(git rev-parse HEAD)" | save commit.txt
cd ..
docker login -u $env.CI_REGISTRY_USER -p $env.CI_REGISTRY_PASSWORD $env.CI_REGISTRY
docker build -t $"registry.gitlab.com/chrisjansson/farss:($env.CI_COMMIT_SHA)" .
docker push $"registry.gitlab.com/chrisjansson/farss:($env.CI_COMMIT_SHA)"
