namespace AppendAuthorizationToDescriptionExample.Services
{ 
    /// <summary>
    /// 
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// The token RSA public key.
        /// </summary>
        /// <remarks>
        /// In any other code repository I would store this in the appsetting.json file as the 
        /// value "StoreInUserSecretsOrAzureWebServiceConfiguration", but this is a test example.
        /// https://cryptotools.net/rsagen
        /// </remarks>
        public string PrivateKey { get; set; }

        /// <summary>
        /// The token RSA public key.
        /// </summary>
        /// <remarks>
        /// In any other code repository I would store this in the appsetting.json file as the 
        /// value "StoreInUserSecretsOrAzureWebServiceConfiguration", but this is a test example.
        /// https://cryptotools.net/rsagen
        /// </remarks>
        public string PublicKey { get; set; }

        /// <summary>
        /// The token issuer.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// The token audience.
        /// </summary>
        public string Audience { get; set; }
    }
}
