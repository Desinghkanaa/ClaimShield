using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ClaimShield.Api.Authentication
{
    // =============================================================
    // Supabase's Auth server only exposes a raw JWKS document
    // (no OIDC discovery document), so the normal
    // OpenIdConnectConfigurationRetriever (which expects a
    // discovery doc with a jwks_uri field) can't be used directly.
    // This retriever treats the address itself as the JWKS URI.
    // =============================================================

    public class SupabaseJwksConfigurationRetriever :
        IConfigurationRetriever<OpenIdConnectConfiguration>
    {
        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
            string address,
            IDocumentRetriever retriever,
            CancellationToken cancel)
        {
            var jwksJson =
                await retriever.GetDocumentAsync(
                    address,
                    cancel);

            var configuration =
                new OpenIdConnectConfiguration
                {
                    JsonWebKeySet =
                        new JsonWebKeySet(jwksJson)
                };

            foreach (var signingKey in
                     configuration.JsonWebKeySet.GetSigningKeys())
            {
                configuration.SigningKeys.Add(signingKey);
            }

            return configuration;
        }
    }
}
