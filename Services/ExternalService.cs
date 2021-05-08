using System.Collections.Generic;
using Sartain_Studios_Common.HttpRestApiCalls;
using Sartain_Studios_Common.Interfaces.Token;
using Sartain_Studios_Common.SharedEntities;
using SharedModels;

namespace Services
{
    public interface IExternalService<TEntity> { }

    public class ExternalService<TEntity> : IExternalService<TEntity>
    {
        protected readonly UserModel _emptyUserModelWithServiceRole = new() {Roles = new List<string> {Role.Service}};
        protected readonly IAutoWrapperHttpClient<TEntity> _httpRequest;
        protected readonly IToken _token;

        public ExternalService(IAutoWrapperHttpClient<TEntity> httpClientWrapper, IToken token)
        {
            _httpRequest = httpClientWrapper;
            _token = token;
        }

        protected string ServiceAccountToken => _token.GenerateToken(_emptyUserModelWithServiceRole);
    }
}