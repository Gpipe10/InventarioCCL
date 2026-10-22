using FluentValidation;
using Inventario.Application.Common;
namespace Inventario.Application.DTOs.Requests;

public sealed record RegistrarEntradaRequest(
    long ProductoId,
    decimal Cantidad,
    decimal CostoUnitario,
    string? Proveedor,
    string? Motivo,
    string? Referencia,
    DateTime? Fecha,
    long UsuarioId
);
