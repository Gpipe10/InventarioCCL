using Inventario.Application.Common;
using Inventario.Domain.Entities;

namespace Inventario.Application.Interfaces.Security;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verificar(string password, string? hash);
}
