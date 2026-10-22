using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Requests;

public sealed record CrearProductoRequest(
    DatosProductoRequest Datos,
    long UsuarioId
);
