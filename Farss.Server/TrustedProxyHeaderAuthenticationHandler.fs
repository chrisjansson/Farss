module Farss.Server.TrustedProxyHeaderAuthenticationHandler

open System.Security.Claims
open Microsoft.AspNetCore.Authentication

type TrustedProxyHeaderAuthenticationHandler(options, logger, encoder) =
    inherit AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)

    let userMapping = [ "chris", "3f914fef-a2dc-4fd5-be13-eff3e5ccd9fb" ] |> Map.ofList

    override this.HandleAuthenticateAsync() =
        let request = base.Request

        task {
            if not (request.Headers.ContainsKey("Remote-User")) then
                return AuthenticateResult.Fail("Authentication failed, missing user header")
            else
                let remoteUser = request.Headers.["Remote-User"].ToString()

                return
                    match Map.tryFind remoteUser userMapping with
                    | None -> AuthenticateResult.Fail("Unknown user")
                    | Some userId ->
                        let claims = [ Claim(ClaimTypes.NameIdentifier, userId) ]

                        let claimsIdentity = ClaimsIdentity(claims, this.Scheme.Name)
                        let claimsPrincipal = ClaimsPrincipal(claimsIdentity)

                        AuthenticateResult.Success(AuthenticationTicket(claimsPrincipal, this.Scheme.Name))
        }
