using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Requests;

public sealed record EditarProductoRequest(
    long Id,
    DatosProductoRequest Datos,
    long UsuarioId
);
