module Postgres

type PostgresConnectionString = 
    {
        Host: string
        Database: string
        Username: string
        Password: string
    }

let createConnectionString (data: PostgresConnectionString) =   
    sprintf "host=%s;database=%s;password=%s;username=%s" data.Host data.Database data.Password data.Username
            