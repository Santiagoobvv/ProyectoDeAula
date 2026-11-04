using DssMultiEmpresa.API.Data;
using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DssMultiEmpresa.API.Services;

/// <summary>
/// Motor de Soporte a Decisiones (DSS) para múltiples empresas.
/// Calcula KPIs, alertas presupuestarias y genera recomendaciones tácticas.
/// </summary>
public class DssService : IDssService
{
    private readonly ApplicationDbContext _db;

    public DssService(ApplicationDbContext db) => _db = db;

    public async Task<DssResumenDto> CalcularResumenAsync(int empresaId, int mes, int anio)
    {
        var empresa = await _db.Empresas.FindAsync(empresaId)
            ?? throw new KeyNotFoundException("Empresa no encontrada.");

        // ── 1. Totales del periodo ──
        var transacciones = await _db.TransaccionesContables
            .Where(t => t.EmpresaId == empresaId &&
                        t.Fecha.Year == anio &&
                        t.Fecha.Month == mes)
            .ToListAsync();

        decimal totalIngresos = transacciones
            .Where(t => t.Tipo == "Ingreso").Sum(t => t.Monto);
        decimal totalGastos = transacciones
            .Where(t => t.Tipo == "Egreso").Sum(t => t.Monto);
        decimal flujoCaja = totalIngresos - totalGastos;

        // ── 2. Margen operativo ──
        decimal margenOperativo = totalIngresos > 0
            ? Math.Round((flujoCaja / totalIngresos) * 100, 2)
            : 0;

        // ── 3. Ejecución presupuestal por categoría ──
        var presupuestos = await _db.Presupuestos
            .Include(p => p.Categoria)
            .Where(p => p.EmpresaId == empresaId &&
                        p.PeriodoAnio == anio &&
                        p.PeriodoMes == mes)
            .ToListAsync();

        var ejecucionList = new List<EjecucionPresupuestalDto>();
        var alertas = new List<AlertaPresupuestariaDto>();

        foreach (var pres in presupuestos)
        {
            // Sumar transacciones de esa categoría en el mes
            decimal ejecutado = transacciones
                .Where(t => t.CategoriaId == pres.CategoriaId)
                .Sum(t => t.Monto);

            decimal porcentaje = pres.MontoPresupuestado > 0
                ? Math.Round((ejecutado / pres.MontoPresupuestado) * 100, 2)
                : 0;

            decimal diferencia = pres.MontoPresupuestado - ejecutado;

            ejecucionList.Add(new EjecucionPresupuestalDto(
                pres.CategoriaId,
                pres.Categoria?.Nombre ?? "Sin categoría",
                pres.Categoria?.Tipo ?? "",
                pres.MontoPresupuestado,
                ejecutado,
                diferencia,
                porcentaje
            ));

            // ── 4. Generar alertas presupuestarias ──
            string nivelAlerta;
            string mensaje;

            if (porcentaje >= 100)
            {
                nivelAlerta = "Rojo";
                mensaje = $"⛔ ALERTA CRÍTICA: '{pres.Categoria?.Nombre}' ha superado el 100% del presupuesto " +
                          $"({porcentaje:F1}%). Se excede en ${Math.Abs(diferencia):N2}.";
            }
            else if (porcentaje >= 85)
            {
                nivelAlerta = "Amarillo";
                mensaje = $"⚠️ PRECAUCIÓN: '{pres.Categoria?.Nombre}' está al {porcentaje:F1}% del presupuesto. " +
                          $"Quedan ${diferencia:N2} disponibles.";
            }
            else
            {
                nivelAlerta = "Verde";
                mensaje = $"✅ '{pres.Categoria?.Nombre}' está en {porcentaje:F1}% — dentro del rango saludable.";
            }

            alertas.Add(new AlertaPresupuestariaDto(
                pres.Categoria?.Nombre ?? "",
                pres.MontoPresupuestado,
                ejecutado,
                porcentaje,
                nivelAlerta,
                mensaje
            ));
        }

        // ── 5. Nivel de alerta global ──
        string nivelGlobal = "Verde";
        if (alertas.Any(a => a.NivelAlerta == "Rojo"))
            nivelGlobal = "Rojo";
        else if (alertas.Any(a => a.NivelAlerta == "Amarillo"))
            nivelGlobal = "Amarillo";

        // Considerar también el flujo de caja negativo
        if (flujoCaja < 0)
            nivelGlobal = "Rojo";
        else if (flujoCaja < totalIngresos * 0.1m && totalIngresos > 0)
            nivelGlobal = nivelGlobal == "Rojo" ? "Rojo" : "Amarillo";

        // ── 6. Generar recomendación táctica ──
        string recomendacion = GenerarRecomendacion(
            flujoCaja, margenOperativo, totalIngresos, totalGastos, alertas);

        // ── 7. Persistir indicador ──
        var indicador = new IndicadorDss
        {
            EmpresaId = empresaId,
            FechaCalculo = DateTime.UtcNow,
            FlujoCaja = flujoCaja,
            MargenOperativo = margenOperativo,
            TotalIngresos = totalIngresos,
            TotalGastos = totalGastos,
            NivelAlerta = nivelGlobal,
            Recomendacion = recomendacion
        };

        _db.IndicadoresDss.Add(indicador);
        await _db.SaveChangesAsync();

        return new DssResumenDto(
            empresaId,
            empresa.NombreEmpresa,
            indicador.FechaCalculo,
            totalIngresos,
            totalGastos,
            flujoCaja,
            margenOperativo,
            nivelGlobal,
            recomendacion,
            alertas,
            ejecucionList
        );
    }

    public async Task<IEnumerable<FlujoCajaMensualDto>> ObtenerFlujoCajaMensualAsync(
        int empresaId, int cantidadMeses = 12)
    {
        var fechaInicio = DateOnly.FromDateTime(DateTime.Today.AddMonths(-cantidadMeses + 1));
        fechaInicio = new DateOnly(fechaInicio.Year, fechaInicio.Month, 1);

        var transacciones = await _db.TransaccionesContables
            .Where(t => t.EmpresaId == empresaId && t.Fecha >= fechaInicio)
            .ToListAsync();

        var resultado = new List<FlujoCajaMensualDto>();

        for (int i = 0; i < cantidadMeses; i++)
        {
            var fecha = DateOnly.FromDateTime(DateTime.Today.AddMonths(-cantidadMeses + 1 + i));
            int mesActual = fecha.Month;
            int anioActual = fecha.Year;

            var delMes = transacciones
                .Where(t => t.Fecha.Year == anioActual && t.Fecha.Month == mesActual);

            decimal ingresos = delMes.Where(t => t.Tipo == "Ingreso").Sum(t => t.Monto);
            decimal egresos = delMes.Where(t => t.Tipo == "Egreso").Sum(t => t.Monto);

            resultado.Add(new FlujoCajaMensualDto(
                mesActual, anioActual, ingresos, egresos, ingresos - egresos
            ));
        }

        return resultado;
    }

    // ═══════════════════════════════════════════════════════════════
    //  GENERADOR DE RECOMENDACIONES TÁCTICAS
    // ═══════════════════════════════════════════════════════════════
    private static string GenerarRecomendacion(
        decimal flujoCaja,
        decimal margenOperativo,
        decimal totalIngresos,
        decimal totalGastos,
        List<AlertaPresupuestariaDto> alertas)
    {
        var partes = new List<string>();

        // Análisis de liquidez
        if (flujoCaja < 0)
        {
            partes.Add("🔴 LIQUIDEZ NEGATIVA: Los egresos superan los ingresos este periodo. " +
                        "Se recomienda revisar de inmediato los gastos no esenciales, " +
                        "negociar plazos de pago con proveedores y acelerar el cobro de cartera.");
        }
        else if (totalIngresos > 0 && margenOperativo < 10)
        {
            partes.Add("🟡 MARGEN BAJO: El margen operativo es inferior al 10%. " +
                        "Considere optimizar costos operativos o diversificar fuentes de ingreso.");
        }
        else if (margenOperativo >= 20)
        {
            partes.Add("🟢 SALUD FINANCIERA SÓLIDA: El margen operativo supera el 20%. " +
                        "Excelente momento para considerar inversiones estratégicas o reservas de contingencia.");
        }
        else
        {
            partes.Add("🟢 Situación financiera estable. Continúe monitoreando los indicadores mensualmente.");
        }

        // Alertas críticas
        var rojas = alertas.Where(a => a.NivelAlerta == "Rojo").ToList();
        if (rojas.Count > 0)
        {
            partes.Add($"⛔ Hay {rojas.Count} categoría(s) con presupuesto superado. " +
                        "Revise las partidas: " +
                        string.Join(", ", rojas.Select(r => r.Categoria)) + ".");
        }

        var amarillas = alertas.Where(a => a.NivelAlerta == "Amarillo").ToList();
        if (amarillas.Count > 0)
        {
            partes.Add($"⚠️ Hay {amarillas.Count} categoría(s) próximas al límite presupuestal. " +
                        "Monitoree de cerca: " +
                        string.Join(", ", amarillas.Select(r => r.Categoria)) + ".");
        }

        // Si no hay ingresos ni gastos
        if (totalIngresos == 0 && totalGastos == 0)
        {
            partes.Clear();
            partes.Add("ℹ️ No hay transacciones registradas para este periodo. " +
                        "Registre ingresos y egresos para obtener un análisis DSS completo.");
        }

        return string.Join("\n\n", partes);
    }
}
