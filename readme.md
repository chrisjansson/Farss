# Farss (_Farce_)

[![Build status](https://ci.appveyor.com/api/projects/status/0acrgm8eq2xjrws0?svg=true)](https://ci.appveyor.com/project/ChrisJansson/farss)

### Tech must haves!
* F#
* Giraffe
* Postgres
* Marten
* Fable
* Elmish
* Reverse proxy support
* PWA client

### Stuff to do

- [x] dotnet test doesn't work with expecto adapter
- [x] CI appveyor
- [ ] Convert CI to a fake script instead
- [ ] Dockerize app
- [ ] Error handling strategy in aspnetcore pipeline
    * Investigate how unhandled errors are surfaced in aspnetcore
    * Does an error handling middleware trap exceptions?
        Giraffe's does.
        How does AI work?
- [ ] Split integration tests that require WebSdk into separate assembly, these don't behave well with normal test integration supplied by expecto runner

#### Subscription management
- [x] POST feed to subscribe
- [x] Aren't feeds really feed subscriptions? 
- [x] GET subscriptions List
- [x] Delete feeds

#### Personal Reading workflow
* Scroll through new articles and mark them as read or read later
* Bookmarklet for articles that arent feed articles into read later
* Save (bookmarket or ui function from read later)

#### Monitoring
* Listing feed errors in the UI would be nice
* Lets start with logging them at all
