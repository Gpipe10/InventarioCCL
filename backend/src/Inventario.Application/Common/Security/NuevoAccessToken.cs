using Inventario.Application.Common;
using Inventario.Domain.Entities;

namespace Inventario.Application.Common.Security;

public sealed record NuevoAccessToken(string Valor, DateTime ExpiraEn);
