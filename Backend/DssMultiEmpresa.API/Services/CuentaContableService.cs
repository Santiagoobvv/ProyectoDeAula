using DssMultiEmpresa.API.Data;
using DssMultiEmpresa.API.DTOs;
using DssMultiEmpresa.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DssMultiEmpresa.API.Services;

public class CuentaContableService : ICuentaContableService
{
    private readonly ApplicationDbContext _db;

    public CuentaContableService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<CuentaContableResponseDto>> GetByEmpresaAsync(
        int empresaId,
        string? busqueda = null,
        int? clase = null,
        string? tipoCategoria = null,
        string? naturaleza = null,
        string? estado = null)
    {
        var query = _db.CuentasContables
            .Include(c => c.Categoria)
            .Where(c => c.EmpresaId == empresaId);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLower();
            query = query.Where(c =>
                c.Codigo.ToLower().Contains(term) ||
                c.Nombre.ToLower().Contains(term) ||
                (c.Descripcion != null && c.Descripcion.ToLower().Contains(term)) ||
                c.TipoCategoria.ToLower().Contains(term)
            );
        }

        if (clase.HasValue && clase.Value > 0)
        {
            query = query.Where(c => c.Clase == clase.Value);
        }

        if (!string.IsNullOrWhiteSpace(tipoCategoria))
        {
            query = query.Where(c => c.TipoCategoria == tipoCategoria);
        }

        if (!string.IsNullOrWhiteSpace(naturaleza))
        {
            query = query.Where(c => c.Naturaleza == naturaleza);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(c => c.Estado == estado);
        }

        return await query
            .OrderBy(c => c.Codigo)
            .Select(c => ToDto(c))
            .ToListAsync();
    }

    public async Task<CuentaContableResponseDto?> GetByIdAsync(int id, int empresaId)
    {
        var cuenta = await _db.CuentasContables
            .Include(c => c.Categoria)
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);

        return cuenta is null ? null : ToDto(cuenta);
    }

    public async Task<CuentaContableResponseDto?> GetByCodigoAsync(string codigo, int empresaId)
    {
        var cuenta = await _db.CuentasContables
            .Include(c => c.Categoria)
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Codigo == codigo);

        return cuenta is null ? null : ToDto(cuenta);
    }

    public async Task<CuentaContableResponseDto> CreateAsync(int empresaId, CuentaContableCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Codigo) || !dto.Codigo.Trim().All(char.IsDigit))
            throw new ArgumentException("El código PUC debe contener exclusivamente números.");
        if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Nombre.Trim().Any(char.IsDigit))
            throw new ArgumentException("El nombre de la cuenta contable solo debe contener letras.");

        var existe = await _db.CuentasContables.AnyAsync(c =>
            c.EmpresaId == empresaId && c.Codigo == dto.Codigo.Trim());

        if (existe)
            throw new InvalidOperationException($"Ya existe una cuenta con el código '{dto.Codigo}' para esta empresa.");

        // Determinar grupo si no se envió
        string? grupo = dto.Grupo;
        if (string.IsNullOrWhiteSpace(grupo) && dto.Codigo.Length >= 2)
        {
            grupo = dto.Codigo.Substring(0, 2);
        }

        var cuenta = new CuentaContable
        {
            EmpresaId = empresaId,
            Codigo = dto.Codigo.Trim(),
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion,
            Clase = dto.Clase,
            Grupo = grupo,
            Nivel = dto.Nivel ?? DeterminarNivel(dto.Codigo),
            Naturaleza = dto.Naturaleza,
            TipoCategoria = dto.TipoCategoria,
            CategoriaId = dto.CategoriaId,
            ManejaTerceros = dto.ManejaTerceros,
            ManejaMovimientos = dto.ManejaMovimientos,
            CuentaPadreCodigo = dto.CuentaPadreCodigo,
            Estado = dto.Estado ?? "Activa"
        };

        _db.CuentasContables.Add(cuenta);
        await _db.SaveChangesAsync();

        if (cuenta.CategoriaId.HasValue)
        {
            await _db.Entry(cuenta).Reference(c => c.Categoria).LoadAsync();
        }

        return ToDto(cuenta);
    }

    public async Task<CuentaContableResponseDto?> UpdateAsync(int id, int empresaId, CuentaContableUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Codigo) || !dto.Codigo.Trim().All(char.IsDigit))
            throw new ArgumentException("El código PUC debe contener exclusivamente números.");
        if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Nombre.Trim().Any(char.IsDigit))
            throw new ArgumentException("El nombre de la cuenta contable solo debe contener letras.");

        var cuenta = await _db.CuentasContables
            .Include(c => c.Categoria)
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);

        if (cuenta is null) return null;

        if (cuenta.Codigo != dto.Codigo.Trim())
        {
            var existe = await _db.CuentasContables.AnyAsync(c =>
                c.EmpresaId == empresaId && c.Codigo == dto.Codigo.Trim() && c.Id != id);
            if (existe)
                throw new InvalidOperationException($"Ya existe otra cuenta con el código '{dto.Codigo}'.");
        }

        cuenta.Codigo = dto.Codigo.Trim();
        cuenta.Nombre = dto.Nombre.Trim();
        cuenta.Descripcion = dto.Descripcion;
        cuenta.Clase = dto.Clase;
        cuenta.Grupo = dto.Grupo ?? (dto.Codigo.Length >= 2 ? dto.Codigo.Substring(0, 2) : cuenta.Grupo);
        cuenta.Nivel = dto.Nivel ?? DeterminarNivel(dto.Codigo);
        cuenta.Naturaleza = dto.Naturaleza;
        cuenta.TipoCategoria = dto.TipoCategoria;
        cuenta.CategoriaId = dto.CategoriaId;
        cuenta.ManejaTerceros = dto.ManejaTerceros;
        cuenta.ManejaMovimientos = dto.ManejaMovimientos;
        cuenta.CuentaPadreCodigo = dto.CuentaPadreCodigo;
        cuenta.Estado = dto.Estado ?? cuenta.Estado;

        await _db.SaveChangesAsync();

        if (cuenta.CategoriaId.HasValue && cuenta.Categoria == null)
        {
            await _db.Entry(cuenta).Reference(c => c.Categoria).LoadAsync();
        }

        return ToDto(cuenta);
    }

    public async Task<bool> ToggleEstadoAsync(int id, int empresaId)
    {
        var cuenta = await _db.CuentasContables
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);

        if (cuenta is null) return false;

        cuenta.Estado = cuenta.Estado == "Activa" ? "Inactiva" : "Activa";
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int empresaId)
    {
        var cuenta = await _db.CuentasContables
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);

        if (cuenta is null) return false;

        // Validar que no tenga transacciones asociadas
        var tieneMovimientos = await _db.TransaccionesContables.AnyAsync(t => t.CuentaContableId == id);
        if (tieneMovimientos)
            throw new InvalidOperationException("No se puede eliminar la cuenta porque tiene transacciones registradas. Puede inactivarla.");

        // Validar que no tenga presupuestos asociados
        var tienePresupuestos = await _db.Presupuestos.AnyAsync(p => p.CuentaContableId == id);
        if (tienePresupuestos)
            throw new InvalidOperationException("No se puede eliminar la cuenta porque tiene presupuestos vinculados.");

        _db.CuentasContables.Remove(cuenta);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> SeedPucBaseAsync(int empresaId)
    {
        var codigosExistentes = await _db.CuentasContables
            .Where(c => c.EmpresaId == empresaId)
            .Select(c => c.Codigo)
            .ToListAsync();
        var existentes = codigosExistentes.ToHashSet();

        var pucColombiano = ObtenerCatalogoPucBase();
        int insertados = 0;

        foreach (var c in pucColombiano)
        {
            if (!existentes.Contains(c.Codigo))
            {
                _db.CuentasContables.Add(new CuentaContable
                {
                    EmpresaId = empresaId,
                    Codigo = c.Codigo,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Clase = c.Clase,
                    Grupo = c.Grupo,
                    Nivel = c.Nivel,
                    Naturaleza = c.Naturaleza,
                    TipoCategoria = c.TipoCategoria,
                    ManejaTerceros = c.ManejaTerceros,
                    ManejaMovimientos = c.ManejaMovimientos,
                    CuentaPadreCodigo = c.CuentaPadreCodigo,
                    Estado = "Activa"
                });
                insertados++;
            }
        }

        if (insertados > 0)
        {
            await _db.SaveChangesAsync();
        }

        return insertados;
    }

    private static string DeterminarNivel(string codigo)
    {
        int len = codigo.Trim().Length;
        return len switch
        {
            1 => "Clase",
            2 => "Grupo",
            4 => "Cuenta",
            6 => "Subcuenta",
            _ => "Auxiliar"
        };
    }

    private static CuentaContableResponseDto ToDto(CuentaContable c) => new(
        c.Id,
        c.EmpresaId,
        c.Codigo,
        c.Nombre,
        c.Descripcion,
        c.Clase,
        c.Grupo,
        c.Nivel,
        c.Naturaleza,
        c.TipoCategoria,
        c.CategoriaId,
        c.Categoria?.Nombre,
        c.ManejaTerceros,
        c.ManejaMovimientos,
        c.CuentaPadreCodigo,
        c.Estado
    );

    // ══════════════════════════════════════════════════════════════════
    //  CATÁLOGO BASE PUC COLOMBIANO PARA EMPRESAS Y CONTADORES
    //  (Decreto 2650 de 1993 y Marco Técnico Normativo NIIF para Pymes)
    // ══════════════════════════════════════════════════════════════════
    private static List<CuentaContableSeedItem> ObtenerCatalogoPucBase()
    {
        return new List<CuentaContableSeedItem>
        {
            // ── CLASE 1: ACTIVO (Naturaleza Débito) ──
            new("1", "Activo", "Recursos controlados por la entidad derivados de sucesos pasados.", 1, null, "Clase", "Debito", "Activo", false, false, null),
            new("11", "Disponible / Efectivo", "Efectivo en caja y depósitos bancarios.", 1, "11", "Grupo", "Debito", "Activo", false, false, "1"),
            new("1105", "Caja", "Dinero en efectivo en poder de la empresa.", 1, "11", "Cuenta", "Debito", "Activo", false, false, "11"),
            new("110505", "Caja General", "Efectivo disponible en caja principal de la empresa.", 1, "11", "Subcuenta", "Debito", "Activo", false, true, "1105"),
            new("110510", "Cajas Menores", "Fondos fijos para gastos menores y urgentes.", 1, "11", "Subcuenta", "Debito", "Activo", true, true, "1105"),
            new("1110", "Bancos", "Depósitos en cuentas corrientes y de ahorros.", 1, "11", "Cuenta", "Debito", "Activo", false, false, "11"),
            new("111005", "Moneda Nacional", "Saldos bancarios en entidades financieras del país.", 1, "11", "Subcuenta", "Debito", "Activo", true, true, "1110"),
            new("13", "Deudores / Cuentas por Cobrar", "Derechos de cobro a favor de la empresa.", 1, "13", "Grupo", "Debito", "Activo", false, false, "1"),
            new("1305", "Clientes", "Créditos otorgados por venta de bienes o prestación de servicios.", 1, "13", "Cuenta", "Debito", "Activo", false, false, "13"),
            new("130505", "Clientes Nacionales", "Cuentas por cobrar comerciales en Colombia.", 1, "13", "Subcuenta", "Debito", "Activo", true, true, "1305"),
            new("1330", "Anticipos y Avances", "Anticipos entregados a proveedores o contratistas.", 1, "13", "Cuenta", "Debito", "Activo", false, false, "13"),
            new("133005", "A Proveedores", "Adelantos entregados por compras o servicios pendientes.", 1, "13", "Subcuenta", "Debito", "Activo", true, true, "1330"),
            new("1355", "Anticipo de Impuestos y Saldos a Favor", "Retenciones en la fuente practicadas y saldos a favor.", 1, "13", "Cuenta", "Debito", "Activo", false, false, "13"),
            new("135515", "Retención en la Fuente a Favor", "Anticipos de retención en la fuente practicados a la empresa.", 1, "13", "Subcuenta", "Debito", "Activo", true, true, "1355"),
            new("14", "Inventarios", "Bienes destinados a la venta o producción.", 1, "14", "Grupo", "Debito", "Activo", false, false, "1"),
            new("1435", "Mercancías No Fabricadas por la Empresa", "Existencias disponibles para comercialización directa.", 1, "14", "Cuenta", "Debito", "Activo", false, true, "14"),
            new("15", "Propiedades, Planta y Equipo", "Activos tangibles para uso en la producción o administración.", 1, "15", "Grupo", "Debito", "Activo", false, false, "1"),
            new("1524", "Equipo de Oficina", "Muebles, enseres y escritorios de trabajo.", 1, "15", "Cuenta", "Debito", "Activo", true, true, "15"),
            new("1528", "Equipo de Computación y Comunicación", "Computadores, servidores y equipos de red.", 1, "15", "Cuenta", "Debito", "Activo", true, true, "15"),
            new("1592", "Depreciación Acumulada", "Desgaste acumulado de las propiedades, planta y equipo.", 1, "15", "Cuenta", "Credito", "Activo", false, true, "15"),

            // ── CLASE 2: PASIVO (Naturaleza Crédito) ──
            new("2", "Pasivo", "Obligaciones presentes contraídas por la entidad.", 2, null, "Clase", "Credito", "Pasivo", false, false, null),
            new("21", "Obligaciones Financieras", "Deudas con entidades de crédito y financiamiento.", 2, "21", "Grupo", "Credito", "Pasivo", false, false, "2"),
            new("2105", "Bancos Nacionales", "Pagarés, sobregiros y préstamos con bancos locales.", 2, "21", "Cuenta", "Credito", "Pasivo", true, true, "21"),
            new("22", "Proveedores", "Obligaciones comerciales por adquisición de bienes o servicios.", 2, "22", "Grupo", "Credito", "Pasivo", false, false, "2"),
            new("2205", "Proveedores Nacionales", "Facturas pendientes de pago a proveedores colombianos.", 2, "22", "Cuenta", "Credito", "Pasivo", true, true, "22"),
            new("23", "Cuentas por Pagar", "Obligaciones varias por costos, gastos y retenciones.", 2, "23", "Grupo", "Credito", "Pasivo", false, false, "2"),
            new("2335", "Costos y Gastos por Pagar", "Cuentas por pagar causadas a terceros.", 2, "23", "Cuenta", "Credito", "Pasivo", false, false, "23"),
            new("233525", "Honorarios por Pagar", "Honorarios causados pendientes de giro.", 2, "23", "Subcuenta", "Credito", "Pasivo", true, true, "2335"),
            new("233550", "Servicios Públicos por Pagar", "Facturas de servicios pendientes de pago.", 2, "23", "Subcuenta", "Credito", "Pasivo", true, true, "2335"),
            new("2365", "Retención en la Fuente", "Retenciones practicadas por la empresa a terceros.", 2, "23", "Cuenta", "Credito", "Pasivo", false, false, "23"),
            new("236515", "Retención por Honorarios", "Retención practicada a personas naturales o jurídicas por honorarios.", 2, "23", "Subcuenta", "Credito", "Pasivo", true, true, "2365"),
            new("236525", "Retención por Servicios", "Retención practicada en pagos por servicios generales.", 2, "23", "Subcuenta", "Credito", "Pasivo", true, true, "2365"),
            new("236540", "Retención por Compras", "Retención en la fuente general sobre adquisiciones.", 2, "23", "Subcuenta", "Credito", "Pasivo", true, true, "2365"),
            new("2367", "Impuesto a las Ventas Retenido (ReteIVA)", "Retención de IVA practicada a proveedores.", 2, "23", "Cuenta", "Credito", "Pasivo", true, true, "23"),
            new("2368", "Impuesto de Industria y Comercio Retenido (ReteICA)", "Retenciones de ICA aplicables en la jurisdicción municipal.", 2, "23", "Cuenta", "Credito", "Pasivo", true, true, "23"),
            new("24", "Impuestos, Gravámenes y Tasas", "Obligaciones fiscales directas con el Estado y la DIAN.", 2, "24", "Grupo", "Credito", "Pasivo", false, false, "2"),
            new("2408", "Impuesto sobre las Ventas por Pagar (IVA)", "Saldo neto entre IVA generado e IVA descontable.", 2, "24", "Cuenta", "Credito", "Pasivo", true, true, "24"),
            new("25", "Obligaciones Laborales", "Saldos pendientes con trabajadores por salarios y prestaciones.", 2, "25", "Grupo", "Credito", "Pasivo", false, false, "2"),
            new("2505", "Salarios por Pagar", "Nómina causada pendiente de pago a empleados.", 2, "25", "Cuenta", "Credito", "Pasivo", true, true, "25"),
            new("2510", "Cesantías Consolidadas", "Prestación social acumulada a liquidar o consignar en fondo.", 2, "25", "Cuenta", "Credito", "Pasivo", true, true, "25"),
            new("2515", "Intereses sobre Cesantías", "12% anual sobre cesantías a pagar directamente en enero.", 2, "25", "Cuenta", "Credito", "Pasivo", true, true, "25"),
            new("2520", "Prima de Servicios", "Prima legal semestral pendiente de pago.", 2, "25", "Cuenta", "Credito", "Pasivo", true, true, "25"),
            new("2525", "Vacaciones Consolidadas", "Descanso remunerado acumulado por los trabajadores.", 2, "25", "Cuenta", "Credito", "Pasivo", true, true, "25"),

            // ── CLASE 3: PATRIMONIO (Naturaleza Crédito) ──
            new("3", "Patrimonio", "Parte residual de los activos una vez deducidos todos los pasivos.", 3, null, "Clase", "Credito", "Patrimonio", false, false, null),
            new("31", "Capital Social", "Aportes de los socios o accionistas al constituir la empresa.", 3, "31", "Grupo", "Credito", "Patrimonio", false, false, "3"),
            new("3105", "Capital Suscrito y Pagado", "Capital social efectivamente cancelado por socios.", 3, "31", "Cuenta", "Credito", "Patrimonio", true, true, "31"),
            new("33", "Reservas", "Apropiaciones de utilidades por ley o estatutos.", 3, "33", "Grupo", "Credito", "Patrimonio", false, false, "3"),
            new("3305", "Reservas Obligatorias", "Reserva legal del 10% de utilidades.", 3, "33", "Cuenta", "Credito", "Patrimonio", false, true, "33"),
            new("36", "Resultados del Ejercicio", "Utilidad o pérdida neta del periodo actual.", 3, "36", "Grupo", "Credito", "Patrimonio", false, false, "3"),
            new("3605", "Utilidad del Ejercicio", "Beneficio positivo acumulado del periodo contable.", 3, "36", "Cuenta", "Credito", "Patrimonio", false, true, "36"),
            new("3610", "Pérdida del Ejercicio", "Resultado negativo del periodo contable.", 3, "36", "Cuenta", "Debito", "Patrimonio", false, true, "36"),
            new("37", "Resultados de Ejercicios Anteriores", "Utilidades acumuladas retenidas o pérdidas de periodos pasados.", 3, "37", "Grupo", "Credito", "Patrimonio", false, true, "3"),

            // ── CLASE 4: INGRESOS (Naturaleza Crédito) ──
            new("4", "Ingresos", "Incrementos en los beneficios económicos producidos a lo largo del periodo.", 4, null, "Clase", "Credito", "Ingreso", false, false, null),
            new("41", "Operacionales", "Ingresos generados por la actividad principal del objeto social.", 4, "41", "Grupo", "Credito", "Ingreso", false, false, "4"),
            new("4135", "Comercio al por Mayor y al por Menor", "Ventas de mercancías y productos comerciales.", 4, "41", "Cuenta", "Credito", "Ingreso", true, true, "41"),
            new("4170", "Actividades de Servicios Profesionales", "Servicios técnicos, profesionales y especializados.", 4, "41", "Cuenta", "Credito", "Ingreso", false, false, "41"),
            new("417005", "Ingresos por Honorarios Profesionales", "Honorarios devengados por trabajos profesionales independientes.", 4, "41", "Subcuenta", "Credito", "Ingreso", true, true, "4170"),
            new("417010", "Ingresos por Servicios Contables y Tributarios", "Servicios de contabilidad, declaraciones de impuestos y exógena.", 4, "41", "Subcuenta", "Credito", "Ingreso", true, true, "4170"),
            new("417015", "Ingresos por Asesoría y Consultoría Financiera", "Asesorías en estructuración financiera, costos y planeación.", 4, "41", "Subcuenta", "Credito", "Ingreso", true, true, "4170"),
            new("42", "No Operacionales", "Ingresos extraordinarios o secundarios.", 4, "42", "Grupo", "Credito", "Ingreso", false, false, "4"),
            new("4210", "Financieros", "Rendimientos sobre depósitos, cuentas e inversiones.", 4, "42", "Cuenta", "Credito", "Ingreso", true, true, "42"),
            new("4295", "Diversos", "Otros ingresos no operacionales.", 4, "42", "Cuenta", "Credito", "Ingreso", true, true, "42"),

            // ── CLASE 5: GASTOS (Naturaleza Débito) ──
            new("5", "Gastos", "Decrementos en los beneficios económicos producidos a lo largo del periodo.", 5, null, "Clase", "Debito", "Gasto", false, false, null),
            new("51", "Operacionales de Administración", "Gastos de gestión administrativa, nómina y servicios.", 5, "51", "Grupo", "Debito", "Gasto", false, false, "5"),
            new("5105", "Gastos de Personal", "Salarios, prestaciones y seguridad social de empleados.", 5, "51", "Cuenta", "Debito", "Gasto", false, false, "51"),
            new("510506", "Sueldos y Salarios", "Remuneración fija mensual pagada a trabajadores.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5105"),
            new("510515", "Horas Extras y Recargos", "Jornada suplementaria, nocturna y festivos.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5105"),
            new("510527", "Auxilio de Transporte", "Subsidio legal de transporte para salarios hasta 2 SMMLV.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5105"),
            new("510530", "Cesantías", "Gasto por cesantías causadas del periodo (8.33%).", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5105"),
            new("510533", "Intereses sobre Cesantías", "Gasto por intereses sobre cesantías (1% mensual).", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5105"),
            new("510536", "Prima de Servicios", "Gasto por prima semestral causada (8.33%).", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5105"),
            new("510539", "Vacaciones", "Gasto por vacaciones causadas (4.17%).", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5105"),
            new("510569", "Aportes a Salud", "Aporte patronal a entidades promotoras de salud.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5105"),
            new("510570", "Aportes a Pensión", "Aporte patronal al fondo de pensiones obligatorio (12%).", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5105"),
            new("510572", "Aportes a Riesgos Laborales (ARL)", "Cotización obligatoria a administradora de riesgos laborales.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5105"),
            new("510578", "Aportes Parafiscales (SENA, ICBF, Caja)", "Aportes a cajas de compensación familiar y parafiscales.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5105"),
            new("5110", "Honorarios", "Remuneración a personas naturales o jurídicas sin relación laboral.", 5, "51", "Cuenta", "Debito", "Gasto", false, false, "51"),
            new("511010", "Asesoría Jurídica", "Servicios de abogados y asesoría en derecho.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5110"),
            new("511030", "Asesoría Financiera y Contable", "Servicios externos de contadores y analistas financieros.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5110"),
            new("511035", "Revisoría Fiscal y Auditoría", "Exámenes de revisor fiscal y auditoría de cuentas.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5110"),
            new("5120", "Arrendamientos", "Cánones de arrendamiento de oficinas o locales.", 5, "51", "Cuenta", "Debito", "Gasto", false, false, "51"),
            new("512010", "Construcciones y Edificaciones / Oficinas", "Arriendo de instalaciones físicas de la empresa.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5120"),
            new("5135", "Servicios", "Servicios públicos, telecomunicaciones y asistencias.", 5, "51", "Cuenta", "Debito", "Gasto", false, false, "51"),
            new("513525", "Acueducto y Alcantarillado", "Facturas de agua y saneamiento básico.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5135"),
            new("513530", "Energía Eléctrica", "Facturas de energía eléctrica.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5135"),
            new("513533", "Gas", "Facturas de gas natural domiciliario o industrial.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5135"),
            new("513535", "Teléfono e Internet", "Planes de conectividad de fibra óptica y telefonía móvil/fija.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5135"),
            new("513550", "Transporte, Fletes y Acarreos", "Gastos de transporte urbano e intermunicipal de la operación.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5135"),
            new("5145", "Mantenimiento y Reparaciones", "Conservación de muebles, equipos e instalaciones.", 5, "51", "Cuenta", "Debito", "Gasto", true, true, "51"),
            new("5195", "Diversos", "Gastos generales, papelería, suscripciones y capacitación.", 5, "51", "Cuenta", "Debito", "Gasto", false, false, "51"),
            new("519525", "Elementos de Aseo y Cafetería", "Artículos de limpieza y cafetería institucional.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5195"),
            new("519530", "Útiles, Papelería y Fotocopias", "Papelería, carpetas, tintas de impresión y fotocopiado.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5195"),
            new("519545", "Educación, Cursos y Diplomados", "Formación profesional continua, diplomados y certificaciones.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5195"),
            new("519565", "Software, Licencias y Servicios en la Nube", "Suscripciones contables, ERP, hosting y almacenamiento web.", 5, "51", "Subcuenta", "Debito", "Gasto", true, true, "5195"),
            new("53", "No Operacionales / Financieros", "Gastos bancarios y financieros.", 5, "53", "Grupo", "Debito", "Gasto", false, false, "5"),
            new("5305", "Financieros", "Intereses, comisiones y gravamen a movimientos financieros.", 5, "53", "Cuenta", "Debito", "Gasto", false, false, "53"),
            new("530505", "Gastos Bancarios y Comisiones", "Comisiones por transferencias, manejo de tarjetas y cheques.", 5, "53", "Subcuenta", "Debito", "Gasto", true, true, "5305"),
            new("530515", "Gravamen a los Movimientos Financieros (4x1000)", "Impuesto bancario del 4x1000 sobre retiros y giros.", 5, "53", "Subcuenta", "Debito", "Gasto", true, true, "5305"),

            // ── CLASE 6: COSTOS DE VENTAS (Naturaleza Débito) ──
            new("6", "Costos de Ventas", "Costo directo incurrido en la prestación de servicios o venta de bienes.", 6, null, "Clase", "Debito", "Costo", false, false, null),
            new("61", "Costo de Ventas y Prestación de Servicios", "Costos imputables directamente al servicio prestado.", 6, "61", "Grupo", "Debito", "Costo", false, false, "6"),
            new("6135", "Costo de Prestación de Servicios Profesionales", "Costos directos de ejecución de contratos y proyectos.", 6, "61", "Cuenta", "Debito", "Costo", true, true, "61"),
            new("613505", "Contratistas y Mano de Obra Directa", "Honorarios y pagos a profesionales directamente asignados.", 6, "61", "Subcuenta", "Debito", "Costo", true, true, "6135")
        };
    }

    private record CuentaContableSeedItem(
        string Codigo,
        string Nombre,
        string Descripcion,
        int Clase,
        string? Grupo,
        string Nivel,
        string Naturaleza,
        string TipoCategoria,
        bool ManejaTerceros,
        bool ManejaMovimientos,
        string? CuentaPadreCodigo
    );
}
