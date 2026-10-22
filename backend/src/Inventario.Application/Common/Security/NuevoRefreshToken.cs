using Inventario.Application.Common;
using Inventario.Domain.Entities;

namespace Inventario.Application.Common.Security;

public sealed record NuevoRefreshToken(string Valor, string Hash);
