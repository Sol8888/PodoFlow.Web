namespace PodoFlow.Web.Seguridad
{
    public class TokenDesarrolloHandler : DelegatingHandler
    {
        private const string Encabezado = "X-Dev-Token";

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _entorno;

        public TokenDesarrolloHandler(IConfiguration configuration, IWebHostEnvironment entorno)
        {
            _configuration = configuration;
            _entorno = entorno;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = _configuration["AutenticacionDesarrollo:Token"];

            if (_entorno.IsDevelopment()
                && !string.IsNullOrWhiteSpace(token)
                && !request.Headers.Contains(Encabezado))
            {
                request.Headers.Add(Encabezado, token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
