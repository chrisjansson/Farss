module Farss.Server.TrustedProxyHeaderAuthenticationHandler

open System.Security.Claims
open Farss.Server.UserCache
open Microsoft.AspNetCore.Authentication

type TrustedProxyHeaderAuthenticationHandler(options, logger, encoder, userCache: UserCache) =
    inherit AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)

    override this.HandleAuthenticateAsync() =
        let request = base.Request

        task {
            if not (request.Headers.ContainsKey("Remote-User")) then
                return AuthenticateResult.Fail("Authentication failed, missing user header")
            else
                let remoteUser = request.Headers.["Remote-User"].ToString()

                let! user = userCache.GetUserAsync(remoteUser.ToLower())

                let userId = user.Id.ToString()
                let claims = [ Claim(ClaimTypes.NameIdentifier, userId) ]

                let claimsIdentity = ClaimsIdentity(claims, this.Scheme.Name)
                let claimsPrincipal = ClaimsPrincipal(claimsIdentity)

                return AuthenticateResult.Success(AuthenticationTicket(claimsPrincipal, this.Scheme.Name))
        }
