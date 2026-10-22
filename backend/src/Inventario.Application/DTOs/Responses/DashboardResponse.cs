using FluentValidation;
using Inventario.Application.Common;
using Inventario.Domain.Enums;

namespace Inventario.Application.DTOs.Responses;

public sealed record DashboardResponse(
    long ProductosActivos,
    decimal StockTotal,
    decimal ValorInventario,
    long ProductosBajoStock,
    IReadOnlyDictionary<string, decimal> StockPorUnidad,
    IReadOnlyList<ActividadMensualResponse> MovimientosPorMes
);

// ======================================================
// VALIDADORES
// ======================================================
