module Postgres

open Microsoft.Extensions.Configuration

type PostgresConnectionString = 
    {
        Host: string
        Database: string
        Username: string
        Password: string
    }

let createConnectionString (data: PostgresConnectionString) =   
    sprintf "host=%s;database=%s;password=%s;username=%s" data.Host data.Database data.Password data.Username
            
let loadConnectionString (configuration: IConfiguration): PostgresConnectionString = 
        let userName = configuration.["postgres:username"]
        let password = configuration.["postgres:password"]
        let host = configuration.["postgres:host"]
        let database = configuration.["postgres:database"]
        {
            Username = userName
            Password = password
            Host = host
            Database = database
        }