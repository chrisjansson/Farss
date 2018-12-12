module Build

open Domain
open System

let article (): Article = { 
    Article.Id = Guid.NewGuid()
    Title = "A title"
    Guid = Guid.NewGuid().ToString()
    Subscription = Guid.NewGuid()
    Content = "Content"
    IsRead = false
}