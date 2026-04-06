namespace Web.Services
{
    public class TokenService
    {
        private readonly Dictionary<string, int> _tokens = new();

        public string GenerateToken(int userId)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) +
                        Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            _tokens[token] = userId;
            return token;
        }

        public int? ValidateToken(string token)
        {
            if (_tokens.TryGetValue(token, out var userId))
                return userId;
            return null;
        }

        public void RevokeToken(string token)
        {
            _tokens.Remove(token);
        }
    }
}
