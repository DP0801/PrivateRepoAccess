using PrivateRepoAccessAPILib.Constants;
using PrivateRepoAccessAPILib.Model;

namespace PrivateRepoAccessAPILib.Contracts
{
    public interface IRepositoryService
    {
        PrivateAccessReponse GetFileContent();
    }
}
