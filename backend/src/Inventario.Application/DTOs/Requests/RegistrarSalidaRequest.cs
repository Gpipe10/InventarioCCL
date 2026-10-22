using FluentValidation;
using Inventario.Application.Common;
namespace Inventario.Application.DTOs.Requests;

public sealed record RegistrarSalidaRequest(
    long ProductoId,
    decimal Cantidad,
    string Motivo,
    string? Referencia,
    DateTime? Fecha,
    long UsuarioId
);

// ======================================================
// DTOs
// ======================================================
