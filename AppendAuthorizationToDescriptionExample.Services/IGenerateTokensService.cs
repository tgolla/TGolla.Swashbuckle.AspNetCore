namespace AppendAuthorizationToDescriptionExample.Services
{
    public interface IGenerateTokensService
    {
        string GenerateToken(string privateKey, string issuer, string audience, uint expires,
            string email, string giveName, string familyName, IList<string> groups);
        string GenerateUserTokenNoGroups();
        string GenerateManageToken();
        string GenerateAdministratorToken();
        string GenerateManagerAdministratorToken();
    }
}