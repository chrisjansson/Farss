module Farss.Server.UserCache

open System
open Entities
open Microsoft.Extensions.Caching.Memory
open ORMappingConfiguration
open Microsoft.EntityFrameworkCore

type UserCache(context: ReaderContext, cache: IMemoryCache) =
    let factory (entry: ICacheEntry) =
        task {
            entry.AbsoluteExpirationRelativeToNow <- (TimeSpan.FromMinutes 10)

            let username = (entry.Key :?> string).ToLower()

            let user =
                task {
                    let! user = context.Users.SingleOrDefaultAsync(fun u -> u.Username.ToLower() = username)

                    if user = null then
                        let user = PersistedUser(Id = Guid.NewGuid(), Username = username)
                        context.Users.AddAsync(user) |> ignore
                        let! _ = context.SaveChangesAsync()
                        return user
                    else
                        return user
                }

            return! user
        }

    member _.GetUserAsync(username: string) =
        cache.GetOrCreateAsync(username, factory)

