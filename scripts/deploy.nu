#!/usr/bin/env nu

let accessToken = $env.PORTAINER_ACCESS_TOKEN
let portainerAddress = $env.PORTAINER_URL
let stackId = $env.PORTAINER_STACK_ID 
let envId = $env.PORTAINER_ENV_ID 
let imageName = $"farss:($env.CI_COMMIT_SHA)" 
let content = open "prod.compose" | str replace "{{farss_image_name}}" $imageName
echo $content

let request = { 
        prune: true 
        pullImage: true
        stackFileContent: $content
    }
    
http put  -k --headers [X-API-Key $accessToken] $"($portainerAddress)/api/stacks/($stackId)?endpointId=($envId)" ($request | to json)
