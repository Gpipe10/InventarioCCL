using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Requests;

public sealed record CambiarEstadoProductoRequest(
    long Id,
    bool Activo,
    long UsuarioId,
    RolUsuario Rol
);

// ======================================================
// FILTROS
// ======================================================
