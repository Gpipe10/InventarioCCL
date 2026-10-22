using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Responses;

public sealed record SesionResponse(
    long UsuarioId,
    string Nombre,
    RolUsuario Rol,
    string AccessToken,
    DateTime AccessExpiraEn,
    string RefreshToken,
    DateTime RefreshExpiraEn
);

// ======================================================
// POL�TICAS DE SEGURIDAD
// ======================================================
