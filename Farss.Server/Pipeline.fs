module Pipeline

open Microsoft.Extensions.DependencyInjection

let resolve pipeline services =
    Resolve.services services pipeline

let runWith arg pipeline =
    pipeline arg

let runInScopeWith pipeline arg (services: IServiceScopeFactory) =
    use scope = services.CreateScope()
    let pipeline = resolve scope.ServiceProvider pipeline    
    pipeline arg

let runInScopeWithAsync pipeline arg (services: IServiceScopeFactory) =
    task {
        use scope = services.CreateAsyncScope()
        let pipeline = resolve scope.ServiceProvider pipeline    
        return! pipeline arg
    }

    
    
