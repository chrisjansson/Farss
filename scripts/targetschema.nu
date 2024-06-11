#!/usr/bin/env nu

def main [from: string, database: string] {
    let config = open "../Farss.Server/appsettings.json"
    let cs = $"host=($config.postgres.host);database=($database);password=($config.postgres.password);username=($config.postgres.username)"
    let fromCs = $"host=($config.postgres.host);database=($from);password=($config.postgres.password);username=($config.postgres.username)"

    dotnet run "setuptargetschema" $fromCs $cs --project "../Farss.Server/Farss.Server.fsproj" 

    let from = $"postgresql://postgres:postgres@postgres/($from)"
    let to = $"postgresql://postgres:postgres@postgres/($database)"

    let script = docker run --network net --rm djrobstep/migra migra $from $to --unsafe --exclude_schema "grate"
    $script
}