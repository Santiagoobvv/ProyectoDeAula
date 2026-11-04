// ═══════════════════════════════════════════════════════════════
// JAS — Panel de Control Financiero y Contable (Colombia)
// Lógica de Frontend Integral (Vanilla JavaScript)
// ═══════════════════════════════════════════════════════════════

const API_BASE = 'http://localhost:5000/api';

// Estado global de la aplicación
const AppState = {
  token: localStorage.getItem('dss_token') || null,
  user: JSON.parse(localStorage.getItem('dss_user') || 'null'),
  empresas: [],
  empresaActivaId: localStorage.getItem('dss_empresa_activa_id') || null,
  categoriasDss: [],
  cuentasContables: [],
  terceros: [],
  centrosCosto: [],
  presupuestos: [],
  transacciones: [],
  filtroClasePuc: null,
  filtroCatPresupuesto: null,
  chartFlujo: null,
  chartDistribucion: null
};

const NOMBRES_MESES = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
];

// ── Inicialización ──
document.addEventListener('DOMContentLoaded', () => {
  poblarSelectoresPeriodo();

  if (AppState.token && AppState.user) {
    mostrarApp();
  } else {
    mostrarLogin();
  }
});

// ═══════════════════════════════════════════════════════════════
// 1. COMUNICACIÓN HTTP & HELPERS
// ═══════════════════════════════════════════════════════════════
function authHeaders() {
  return {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${AppState.token}`
  };
}

async function apiRequest(endpoint, method = 'GET', body = null) {
  try {
    const options = {
      method,
      headers: authHeaders()
    };
    if (body) {
      options.body = JSON.stringify(body);
    }

    const response = await fetch(`${API_BASE}${endpoint}`, options);

    if (response.status === 401) {
      showToast('Sesión vencida. Inicie sesión nuevamente.', 'error');
      logout();
      return null;
    }

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      const msg = errorData.error || `Error ${response.status}: ${response.statusText}`;
      showToast(msg, 'error');
      return null;
    }

    if (response.status === 204) return true;
    return await response.json();
  } catch (err) {
    console.error('Error en fetch:', err);
    showToast('No se pudo conectar con el servidor backend JAS.', 'error');
    return null;
  }
}

function showToast(message, type = 'info') {
  const container = document.getElementById('toast-container');
  if (!container) return;
  const toast = document.createElement('div');
  toast.className = `toast ${type}`;
  toast.textContent = message;
  container.appendChild(toast);
  setTimeout(() => toast.remove(), 4000);
}

function formatMoneda(valor) {
  const num = Number(valor) || 0;
  return new Intl.NumberFormat('es-CO', {
    style: 'currency',
    currency: 'COP',
    maximumFractionDigits: 0
  }).format(num);
}

// Algoritmo oficial de Dígito de Verificación (DIAN Colombia - Art. 555-2 E.T.)
function calcularDv(nit) {
  if (!nit) return '';
  const digitos = String(nit).replace(/\D/g, '');
  if (!digitos.length) return '';

  const factores = [3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71];
  let suma = 0;
  for (let i = 0; i < digitos.length && i < factores.length; i++) {
    const d = parseInt(digitos.charAt(digitos.length - 1 - i), 10);
    suma += d * factores[i];
  }
  const residuo = suma % 11;
  if (residuo === 0) return '0';
  if (residuo === 1) return '1';
  return String(11 - residuo);
}

function poblarSelectoresPeriodo() {
  const mesActual = new Date().getMonth() + 1;
  const selectores = ['dash-mes', 'tx-filter-mes', 'pres-mes', 'pres-filter-mes'];

  selectores.forEach(id => {
    const el = document.getElementById(id);
    if (!el) return;
    el.innerHTML = '';
    NOMBRES_MESES.forEach((nombre, idx) => {
      const opt = document.createElement('option');
      opt.value = idx + 1;
      opt.textContent = nombre;
      if (idx + 1 === mesActual) opt.selected = true;
      el.appendChild(opt);
    });
  });

  const anioActual = new Date().getFullYear();
  ['dash-anio', 'tx-filter-anio', 'pres-anio', 'pres-filter-anio'].forEach(id => {
    const el = document.getElementById(id);
    if (el) el.value = anioActual;
  });

  const txFecha = document.getElementById('tx-fecha');
  if (txFecha) {
    txFecha.value = new Date().toISOString().split('T')[0];
  }
}

// ═══════════════════════════════════════════════════════════════
// 2. AUTENTICACIÓN
// ═══════════════════════════════════════════════════════════════
function switchAuthTab(tab) {
  const isLogin = tab === 'login';
  document.getElementById('tab-login').classList.toggle('active', isLogin);
  document.getElementById('tab-register').classList.toggle('active', !isLogin);
  document.getElementById('form-login').style.display = isLogin ? 'block' : 'none';
  document.getElementById('form-register').style.display = isLogin ? 'none' : 'block';
}

async function handleLogin(e) {
  e.preventDefault();
  const email = document.getElementById('login-email').value.trim();
  const password = document.getElementById('login-password').value;

  try {
    const res = await fetch(`${API_BASE}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });

    const data = await res.json();
    if (!res.ok) {
      showToast(data.error || 'Credenciales inválidas', 'error');
      return;
    }

    guardarSesion(data);
    showToast(`Bienvenido contador(a) ${data.nombre}`, 'success');
    mostrarApp();
  } catch (err) {
    showToast('Error al conectar con la API de autenticación', 'error');
  }
}

async function handleRegister(e) {
  e.preventDefault();
  const nombre = document.getElementById('reg-name').value.trim();
  const email = document.getElementById('reg-email').value.trim();
  const password = document.getElementById('reg-password').value;

  try {
    const res = await fetch(`${API_BASE}/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ nombre, email, password })
    });

    const data = await res.json();
    if (!res.ok) {
      showToast(data.error || 'Error al registrar usuario', 'error');
      return;
    }

    guardarSesion(data);
    showToast(`Cuenta creada con éxito. Bienvenido(a) ${data.nombre}`, 'success');
    mostrarApp();
  } catch (err) {
    showToast('Error al conectar con la API de registro', 'error');
  }
}

function guardarSesion(data) {
  AppState.token = data.token;
  AppState.user = { id: data.usuarioId, nombre: data.nombre, email: data.email };
  localStorage.setItem('dss_token', data.token);
  localStorage.setItem('dss_user', JSON.stringify(AppState.user));
}

function logout() {
  localStorage.removeItem('dss_token');
  localStorage.removeItem('dss_user');
  localStorage.removeItem('dss_empresa_activa_id');
  AppState.token = null;
  AppState.user = null;
  AppState.empresaActivaId = null;
  AppState.empresas = [];
  mostrarLogin();
}

function mostrarLogin() {
  document.getElementById('login-page').style.display = 'flex';
  document.getElementById('app-layout').classList.remove('visible');
}

async function mostrarApp() {
  document.getElementById('login-page').style.display = 'none';
  document.getElementById('app-layout').classList.add('visible');

  document.getElementById('user-name').textContent = AppState.user.nombre;
  document.getElementById('user-email').textContent = AppState.user.email;
  document.getElementById('user-avatar').textContent = AppState.user.nombre.charAt(0).toUpperCase();

  await cargarEmpresas();
}

// ═══════════════════════════════════════════════════════════════
// 3. NAVEGACIÓN Y GESTIÓN DE EMPRESA ACTIVA
// ═══════════════════════════════════════════════════════════════
function navigateTo(sectionId) {
  document.querySelectorAll('.sidebar-nav button').forEach(btn => {
    btn.classList.toggle('active', btn.dataset.section === sectionId);
  });

  document.querySelectorAll('.main-content > .section').forEach(sec => {
    sec.classList.remove('active');
  });

  const target = document.getElementById(`section-${sectionId}`);
  if (target) target.classList.add('active');

  const sidebar = document.getElementById('sidebar');
  if (sidebar) sidebar.classList.remove('open');

  // Recargas según sección seleccionada
  if (sectionId === 'dashboard') loadDashboard();
  if (sectionId === 'transacciones') loadTransacciones();
  if (sectionId === 'presupuestos') loadPresupuestos();
  if (sectionId === 'cuentas') loadCuentasContables();
  if (sectionId === 'terceros') loadTerceros();
  if (sectionId === 'centros-costo') loadCentrosCosto();
  if (sectionId === 'empresas') renderEmpresasCards();
}

function toggleSidebar() {
  document.getElementById('sidebar').classList.toggle('open');
}

async function cargarEmpresas() {
  const data = await apiRequest('/empresas');
  if (!data) return;

  AppState.empresas = data;
  const select = document.getElementById('empresa-activa');
  select.innerHTML = '<option value="">— Seleccione empresa —</option>';

  data.forEach(emp => {
    const opt = document.createElement('option');
    opt.value = emp.id;
    const docDisplay = emp.nit ? ` (${emp.nit})` : (emp.numeroDocumento ? ` (${emp.numeroDocumento})` : '');
    opt.textContent = emp.nombreEmpresa + docDisplay;
    select.appendChild(opt);
  });

  if (data.length > 0) {
    const guardada = AppState.empresaActivaId;
    const existe = data.some(e => e.id == guardada);
    const idAUsar = existe ? guardada : data[0].id;

    select.value = idAUsar;
    AppState.empresaActivaId = idAUsar;
    localStorage.setItem('dss_empresa_activa_id', idAUsar);

    await onEmpresaCambiada();
  } else {
    actualizarNombreEmpresaDashboard(null);
    navigateTo('empresas');
    abrirFormularioEmpresa();
    showToast('Comience registrando su primera empresa o cliente.', 'info');
  }
}

async function onEmpresaChange() {
  const select = document.getElementById('empresa-activa');
  AppState.empresaActivaId = select.value || null;
  localStorage.setItem('dss_empresa_activa_id', AppState.empresaActivaId || '');
  await onEmpresaCambiada();
}

async function onEmpresaCambiada() {
  if (!AppState.empresaActivaId) {
    actualizarNombreEmpresaDashboard(null);
    return;
  }

  const emp = AppState.empresas.find(e => e.id == AppState.empresaActivaId);
  actualizarNombreEmpresaDashboard(emp ? emp.nombreEmpresa : null);

  // Carga paralela de catálogos de la empresa
  await Promise.all([
    cargarCategoriasDss(),
    cargarCuentasContablesEmpresa(),
    cargarTercerosEmpresa(),
    cargarCentrosCostoEmpresa()
  ]);

  await loadDashboard();
}

function actualizarNombreEmpresaDashboard(nombre) {
  const el = document.getElementById('dashboard-empresa-name');
  if (el) {
    el.textContent = nombre
      ? `Indicadores financieros y soporte a decisiones para: ${nombre}`
      : 'Seleccione una empresa activa para ver su análisis contable y financiero';
  }
}

// ═══════════════════════════════════════════════════════════════
// 4. MÓDULO DASHBOARD DSS & SOPORTE A DECISIONES
// ═══════════════════════════════════════════════════════════════
async function loadDashboard() {
  if (!AppState.empresaActivaId) return;

  const mes = document.getElementById('dash-mes').value;
  const anio = document.getElementById('dash-anio').value;

  const resumen = await apiRequest(`/empresas/${AppState.empresaActivaId}/dss/resumen?mes=${mes}&anio=${anio}`);
  const flujoMensual = await apiRequest(`/empresas/${AppState.empresaActivaId}/dss/flujo-caja?meses=6`);

  if (resumen) {
    document.getElementById('kpi-ingresos').textContent = formatMoneda(resumen.totalIngresos);
    document.getElementById('kpi-gastos').textContent = formatMoneda(resumen.totalGastos);

    const flujoEl = document.getElementById('kpi-flujo');
    flujoEl.textContent = formatMoneda(resumen.flujoCaja);
    flujoEl.classList.toggle('positive', resumen.flujoCaja >= 0);
    flujoEl.classList.toggle('negative', resumen.flujoCaja < 0);

    const alertaBadge = document.getElementById('kpi-alerta');
    const colorClass = resumen.nivelAlerta.toLowerCase();
    alertaBadge.innerHTML = `<span class="alert-badge ${colorClass}">● ${resumen.nivelAlerta}</span>`;

    document.getElementById('dss-recomendacion').textContent = resumen.recomendacion;

    renderEjecucionPresupuestal(resumen.ejecucionPresupuestal);
    renderChartDistribucion(resumen.totalIngresos, resumen.totalGastos);
  }

  if (flujoMensual) {
    renderChartFlujo(flujoMensual);
  }
}

function renderEjecucionPresupuestal(items) {
  const list = document.getElementById('ejecucion-list');
  if (!items || items.length === 0) {
    list.innerHTML = `
      <div class="empty-state">
        <div class="empty-icon"><i class="bi bi-clipboard-data"></i></div>
        <p>No hay presupuestos definidos para este periodo. Vaya a <b>Presupuestos</b> para asignarlos.</p>
      </div>`;
    return;
  }

  list.innerHTML = items.map(it => {
    let color = 'verde';
    if (it.porcentajeEjecucion >= 100) color = 'rojo';
    else if (it.porcentajeEjecucion >= 85) color = 'amarillo';

    const pctVisual = Math.min(it.porcentajeEjecucion, 100);

    return `
      <div class="budget-item">
        <div class="budget-header">
          <span class="cat-name">${it.categoriaNombre} <small style="color:var(--text-muted)">(${it.categoriaTipo})</small></span>
          <span class="budget-pct" style="color: var(--alert-${color})">${it.porcentajeEjecucion}%</span>
        </div>
        <div class="budget-progress">
          <div class="bar ${color}" style="width: ${pctVisual}%"></div>
        </div>
        <div class="budget-details">
          <span>Ejecutado: <b>${formatMoneda(it.ejecutado)}</b></span>
          <span>Presupuestado: ${formatMoneda(it.presupuestado)}</span>
          <span>Diferencia: ${formatMoneda(it.diferencia)}</span>
        </div>
      </div>
    `;
  }).join('');
}

function renderChartFlujo(datos) {
  const ctx = document.getElementById('chart-flujo');
  if (!ctx) return;

  const labels = datos.map(d => `${NOMBRES_MESES[d.mes - 1].substring(0, 3)} ${d.anio}`);
  const ingresos = datos.map(d => d.ingresos);
  const egresos = datos.map(d => d.egresos);
  const flujo = datos.map(d => d.flujoCaja);

  if (AppState.chartFlujo) {
    AppState.chartFlujo.destroy();
  }

  AppState.chartFlujo = new Chart(ctx, {
    type: 'bar',
    data: {
      labels,
      datasets: [
        {
          label: 'Ingresos',
          data: ingresos,
          backgroundColor: 'rgba(64, 192, 87, 0.75)',
          borderRadius: 6
        },
        {
          label: 'Gastos',
          data: egresos,
          backgroundColor: 'rgba(250, 82, 82, 0.75)',
          borderRadius: 6
        },
        {
          label: 'Flujo Neto',
          data: flujo,
          type: 'line',
          borderColor: '#5c7cfa',
          borderWidth: 3,
          pointBackgroundColor: '#5c7cfa',
          tension: 0.3
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { labels: { color: '#adb5bd', font: { family: 'Inter' } } }
      },
      scales: {
        x: { ticks: { color: '#868e96' }, grid: { color: 'rgba(255,255,255,0.05)' } },
        y: { ticks: { color: '#868e96' }, grid: { color: 'rgba(255,255,255,0.05)' } }
      }
    }
  });
}

function renderChartDistribucion(ingresos, gastos) {
  const ctx = document.getElementById('chart-distribucion');
  if (!ctx) return;

  if (AppState.chartDistribucion) {
    AppState.chartDistribucion.destroy();
  }

  AppState.chartDistribucion = new Chart(ctx, {
    type: 'doughnut',
    data: {
      labels: ['Ingresos', 'Gastos'],
      datasets: [{
        data: [Number(ingresos) || 0, Number(gastos) || 0],
        backgroundColor: ['#40c057', '#fa5252'],
        borderWidth: 0
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { position: 'bottom', labels: { color: '#adb5bd' } }
      }
    }
  });
}

// ═══════════════════════════════════════════════════════════════
// 5. MÓDULO DE TRANSACCIONES CONTABLES
// ═══════════════════════════════════════════════════════════════
async function loadTransacciones() {
  if (!AppState.empresaActivaId) return;

  const mes = document.getElementById('tx-filter-mes').value;
  const anio = document.getElementById('tx-filter-anio').value;
  const tipo = document.getElementById('tx-filter-tipo').value;

  let url = `/empresas/${AppState.empresaActivaId}/transacciones?mes=${mes}&anio=${anio}`;
  if (tipo) url += `&tipo=${tipo}`;

  const data = await apiRequest(url);
  AppState.transacciones = data || [];
  renderTransaccionesTable(AppState.transacciones);
}

function renderTransaccionesTable(items) {
  const tbody = document.getElementById('transacciones-body');
  if (!tbody) return;

  if (!items || items.length === 0) {
    tbody.innerHTML = '<tr><td colspan="10" class="empty-state" style="padding:2.5rem">Sin transacciones registradas para este periodo</td></tr>';
    return;
  }

  tbody.innerHTML = items.map(t => {
    const cuentaDisplay = t.codigoCuenta
      ? `<span class="badge-code">${t.codigoCuenta}</span> <b>${t.nombreCuenta || ''}</b>`
      : (t.categoriaNombre || '—');

    const terceroDisplay = t.terceroNombre
      ? `<div><b>${t.terceroNombre}</b>${t.terceroNit ? `<br><small style="color:var(--text-muted)">NIT: ${t.terceroNit}</small>` : ''}</div>`
      : '<span style="color:var(--text-muted)">—</span>';

    const soporteDisplay = t.tipoDocumentoSoporte
      ? `<br><span class="doc-tag">${t.tipoDocumentoSoporte} ${t.numeroDocumentoSoporte ? `(${t.numeroDocumentoSoporte})` : ''}</span>`
      : '';

    const debitoDisplay = t.montoDebito > 0 ? formatMoneda(t.montoDebito) : '—';
    const creditoDisplay = t.montoCredito > 0 ? formatMoneda(t.montoCredito) : '—';

    return `
      <tr>
        <td style="white-space:nowrap">${t.fecha}</td>
        <td><small style="color:var(--primary-300); font-family:monospace">${t.numeroTransaccion || 'TX-' + t.id}</small></td>
        <td>${cuentaDisplay}</td>
        <td>${terceroDisplay}</td>
        <td>${t.centroCostoNombre ? `<span class="badge-code">${t.centroCostoNombre}</span>` : '<span style="color:var(--text-muted)">—</span>'}</td>
        <td>
          <b>${t.concepto}</b>
          ${t.descripcion && t.descripcion !== t.concepto ? `<br><small style="color:var(--text-secondary)">${t.descripcion}</small>` : ''}
          ${soporteDisplay}
        </td>
        <td style="text-align:right" class="monto-debito">${debitoDisplay}</td>
        <td style="text-align:right" class="monto-credito">${creditoDisplay}</td>
        <td style="text-align:right"><b>${formatMoneda(t.monto)}</b></td>
        <td>
          <button class="btn btn-danger btn-sm" onclick="eliminarTransaccion(${t.id})" title="Eliminar transacción" aria-label="Eliminar transacción">
            <i class="bi bi-trash"></i>
          </button>
        </td>
      </tr>
    `;
  }).join('');
}

function filtrarTransacciones() {
  const query = (document.getElementById('tx-search-input').value || '').trim().toLowerCase();
  if (!query) {
    renderTransaccionesTable(AppState.transacciones);
    return;
  }

  const filtradas = AppState.transacciones.filter(t =>
    (t.concepto && t.concepto.toLowerCase().includes(query)) ||
    (t.descripcion && t.descripcion.toLowerCase().includes(query)) ||
    (t.codigoCuenta && t.codigoCuenta.toLowerCase().includes(query)) ||
    (t.nombreCuenta && t.nombreCuenta.toLowerCase().includes(query)) ||
    (t.terceroNombre && t.terceroNombre.toLowerCase().includes(query)) ||
    (t.terceroNit && t.terceroNit.toLowerCase().includes(query)) ||
    (t.numeroTransaccion && t.numeroTransaccion.toLowerCase().includes(query)) ||
    (t.numeroDocumentoSoporte && t.numeroDocumentoSoporte.toLowerCase().includes(query))
  );

  renderTransaccionesTable(filtradas);
}

function limpiarFiltrosTransacciones() {
  document.getElementById('tx-search-input').value = '';
  document.getElementById('tx-filter-tipo').value = '';
  loadTransacciones();
}

function onTxCuentaSelectChange() {
  const select = document.getElementById('tx-cuenta-select');
  const badge = document.getElementById('tx-cuenta-info-badge');
  const reqIndicator = document.getElementById('tx-tercero-required-indicator');
  const naturalezaSelect = document.getElementById('tx-naturaleza');
  const tipoSelect = document.getElementById('tx-tipo');

  if (!select.value) {
    if (badge) badge.textContent = '';
    if (reqIndicator) reqIndicator.style.display = 'none';
    return;
  }

  const cuenta = AppState.cuentasContables.find(c => c.id == select.value);
  if (!cuenta) return;

  if (!cuenta.manejaMovimientos) {
    showToast(`⚠️ La cuenta ${cuenta.codigo} es de agrupación. Seleccione una subcuenta para imputar movimientos.`, 'error');
  }

  naturalezaSelect.value = cuenta.naturaleza || 'Debito';
  if (cuenta.clase === 4 || cuenta.tipoCategoria === 'Ingreso') {
    tipoSelect.value = 'Ingreso';
  } else {
    tipoSelect.value = 'Egreso';
  }

  if (cuenta.manejaTerceros) {
    reqIndicator.style.display = 'inline';
    badge.innerHTML = `<span class="badge-tercero">Requiere Tercero</span> <span class="badge-${cuenta.naturaleza.toLowerCase()}">${cuenta.naturaleza}</span>`;
  } else {
    reqIndicator.style.display = 'none';
    badge.innerHTML = `<span class="badge-${cuenta.naturaleza.toLowerCase()}">${cuenta.naturaleza}</span>`;
  }
}

function onTxTerceroSelectChange() {
  const select = document.getElementById('tx-tercero-select');
  const manualInput = document.getElementById('tx-tercero-manual');
  if (!select.value) return;

  const t = AppState.terceros.find(item => item.id == select.value);
  if (t && manualInput) {
    manualInput.value = t.numeroDocumento + (t.digitoVerificacion ? `-${t.digitoVerificacion}` : '');
  }
}

function abrirModalTransaccion() {
  const modal = document.getElementById('modal-transaccion');
  const form = document.getElementById('form-transaccion');

  if (!modal || !form) {
    console.error('No se encontró el modal de transacciones.');
    return;
  }

  if (!AppState.empresaActivaId) {
    showToast('Seleccione una empresa activa primero', 'error');
    return;
  }

  form.reset();

  // Establecer la fecha actual
  document.getElementById('tx-fecha').value =
    new Date().toISOString().split('T')[0];

  // Limpiar datos auxiliares
  document.getElementById('tx-tercero-manual').value = '';
  document.getElementById('tx-cuenta-info-badge').textContent = '';

  modal.classList.add('open');
}

function cerrarModalTransaccion() {
  const modal = document.getElementById('modal-transaccion');

  if (modal) {
    modal.classList.remove('open');
  }
}

async function crearTransaccion(e) {
  e.preventDefault();
  if (!AppState.empresaActivaId) {
    showToast('Seleccione una empresa activa primero', 'error');
    return;
  }

  const cuentaId = document.getElementById('tx-cuenta-select').value;
  const cuentaObj = AppState.cuentasContables.find(c => c.id == cuentaId);

  if (cuentaObj && !cuentaObj.manejaMovimientos) {
    showToast(`La cuenta '${cuentaObj.codigo}' es de agrupación. Debe seleccionar una subcuenta o auxiliar válida.`, 'error');
    return;
  }

  const terceroId = document.getElementById('tx-tercero-select').value || null;
  const terceroManual = document.getElementById('tx-tercero-manual').value.trim() || null;

  if (cuentaObj && cuentaObj.manejaTerceros && !terceroId && !terceroManual) {
    showToast(`La cuenta '${cuentaObj.codigo}' exige especificar el tercero responsable.`, 'error');
    return;
  }

  const fecha = document.getElementById('tx-fecha').value;
  const numeroTransaccion = document.getElementById('tx-numero').value.trim() || null;
  const tipo = document.getElementById('tx-tipo').value;
  const naturaleza = document.getElementById('tx-naturaleza').value;
  const monto = parseFloat(document.getElementById('tx-monto').value);

  const docTipo = document.getElementById('tx-doc-tipo').value || null;
  const docNumero = document.getElementById('tx-doc-numero').value.trim() || null;
  const centroCostoId = document.getElementById('tx-centro-costo').value || null;

  const concepto = document.getElementById('tx-concepto').value.trim();
  const descripcion = document.getElementById('tx-descripcion').value.trim() || null;

  const payload = {
    fecha,
    numeroTransaccion,
    cuentaContableId: cuentaId ? parseInt(cuentaId) : null,
    codigoCuenta: cuentaObj ? cuentaObj.codigo : null,
    nombreCuenta: cuentaObj ? cuentaObj.nombre : null,
    terceroId: terceroId ? parseInt(terceroId) : null,
    terceroNit: terceroManual,
    centroCostoId: centroCostoId ? parseInt(centroCostoId) : null,
    tipo,
    naturalezaMovimiento: naturaleza,
    monto,
    montoDebito: naturaleza === 'Debito' ? monto : 0,
    montoCredito: naturaleza === 'Credito' ? monto : 0,
    tipoDocumentoSoporte: docTipo,
    numeroDocumentoSoporte: docNumero,
    concepto,
    descripcion
  };

  const res = await apiRequest(`/empresas/${AppState.empresaActivaId}/transacciones`, 'POST', payload);
  if (res) {
    showToast('Transacción contable registrada con éxito', 'success');
    cerrarModalTransaccion();
    document.getElementById('tx-concepto').value = '';
    document.getElementById('tx-descripcion').value = '';
    document.getElementById('tx-monto').value = '';
    document.getElementById('tx-numero').value = '';
    document.getElementById('tx-doc-numero').value = '';
    document.getElementById('tx-tercero-manual').value = '';

    await loadTransacciones();
    await loadDashboard();
  }
}

async function eliminarTransaccion(id) {
  if (!confirm('¿Confirma la anulación/eliminación de este movimiento contable?')) return;
  const res = await apiRequest(`/empresas/${AppState.empresaActivaId}/transacciones/${id}`, 'DELETE');
  if (res) {
    showToast('Movimiento contable eliminado', 'success');
    await loadTransacciones();
    await loadDashboard();
  }
}

// ═══════════════════════════════════════════════════════════════
// 6. MÓDULO DE PLANIFICACIÓN Y PRESUPUESTOS
// ═══════════════════════════════════════════════════════════════
async function loadPresupuestos() {
  if (!AppState.empresaActivaId) return;

  const mes = document.getElementById('pres-filter-mes').value;
  const anio = document.getElementById('pres-filter-anio').value;
  const cat = document.getElementById('pres-filter-cat').value;

  let url = `/empresas/${AppState.empresaActivaId}/presupuestos?mes=${mes}&anio=${anio}`;
  if (cat) url += `&categoria=${encodeURIComponent(cat)}`;

  const data = await apiRequest(url);
  AppState.presupuestos = data || [];
  actualizarIndicadoresPresupuesto(AppState.presupuestos);
  renderPresupuestosTable(AppState.presupuestos);
}

function actualizarIndicadoresPresupuesto(items) {
  const categorias = ['Honorarios', 'Servicios', 'Personal', 'Educación', 'Operación', 'Ingresos'];
  categorias.forEach(cat => {
    const slug = cat.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    const partidas = items.filter(p => (p.categoriaSeccion || p.categoriaNombre || '').toLowerCase().includes(slug));
    const total = partidas.reduce((acc, p) => acc + (Number(p.montoPresupuestado) || 0), 0);

    const amountEl = document.getElementById(`cat-amount-${slug}`);
    const countEl = document.getElementById(`cat-count-${slug}`);
    if (amountEl) amountEl.textContent = formatMoneda(total);
    if (countEl) countEl.textContent = `${partidas.length} partida${partidas.length === 1 ? '' : 's'}`;
  });
}

function filtrarPresupuestoPorCategoria(categoria) {
  document.getElementById('pres-filter-cat').value = categoria;
  loadPresupuestos();
}

function renderPresupuestosTable(items) {
  const tbody = document.getElementById('presupuestos-body');
  if (!tbody) return;

  if (!items || items.length === 0) {
    tbody.innerHTML = '<tr><td colspan="8" class="empty-state" style="padding:2.5rem">Sin presupuestos configurados para los filtros seleccionados</td></tr>';
    return;
  }

  tbody.innerHTML = items.map(p => `
    <tr>
      <td><span class="badge-code">${p.codigo || 'PRE-' + p.id}</span></td>
      <td>
        <b>${p.nombre || 'Presupuesto'}</b>
        ${p.descripcion ? `<br><small style="color:var(--text-secondary)">${p.descripcion}</small>` : ''}
      </td>
      <td><span class="alert-badge verde">${p.categoriaSeccion || p.categoriaNombre || 'General'}</span></td>
      <td>${p.cuentaContableCodigo ? `<span class="badge-code">${p.cuentaContableCodigo}</span> ${p.cuentaContableNombre || ''}` : '<span style="color:var(--text-muted)">—</span>'}</td>
      <td>${NOMBRES_MESES[p.periodoMes - 1]} ${p.periodoAnio}</td>
      <td><small style="color:var(--primary-300)">${p.tipoPresupuesto || 'Gasto'}</small></td>
      <td style="text-align:right"><b>${formatMoneda(p.montoPresupuestado)}</b></td>
      <td>
        <div class="btn-group">
          <button class="btn btn-secondary btn-sm" onclick="editarPresupuesto(${p.id})" title="Editar presupuesto">
            <i class="bi bi-pencil"></i>
          </button>
          <button class="btn btn-danger btn-sm" onclick="eliminarPresupuesto(${p.id})" title="Eliminar presupuesto">
            <i class="bi bi-trash"></i>
          </button>
        </div>
      </td>
    </tr>
  `).join('');
}

function filtrarPresupuestos() {
  const query = (document.getElementById('pres-search-input').value || '').trim().toLowerCase();
  if (!query) {
    renderPresupuestosTable(AppState.presupuestos);
    return;
  }

  const filtrados = AppState.presupuestos.filter(p =>
    (p.codigo && p.codigo.toLowerCase().includes(query)) ||
    (p.nombre && p.nombre.toLowerCase().includes(query)) ||
    (p.descripcion && p.descripcion.toLowerCase().includes(query)) ||
    (p.categoriaNombre && p.categoriaNombre.toLowerCase().includes(query)) ||
    (p.cuentaContableCodigo && p.cuentaContableCodigo.toLowerCase().includes(query)) ||
    (p.cuentaContableNombre && p.cuentaContableNombre.toLowerCase().includes(query))
  );

  renderPresupuestosTable(filtrados);
}

function limpiarFiltrosPresupuesto() {
  document.getElementById('pres-search-input').value = '';
  document.getElementById('pres-filter-cat').value = '';
  loadPresupuestos();
}

function toggleFormPresupuesto(data = null) {
  const modal = document.getElementById('modal-presupuesto');
  const form = document.getElementById('form-presupuesto');
  const title = document.getElementById('pres-form-title');

  if (!modal || !form || !title) {
    console.error('No se encontró el modal o alguno de los elementos del presupuesto.');
    return;
  }

  if (data) {
    // Modo edición
    title.innerHTML = '<i class="bi bi-pencil-square"></i> Editar Presupuesto';

    document.getElementById('pres-id').value = data.id;
    document.getElementById('pres-codigo').value = data.codigo || '';
    document.getElementById('pres-nombre').value = data.nombre || '';
    document.getElementById('pres-categoria-seccion').value =
      data.categoriaSeccion || data.categoriaNombre || 'Honorarios';
    document.getElementById('pres-mes').value = data.periodoMes || '';
    document.getElementById('pres-anio').value = data.periodoAnio || '';
    document.getElementById('pres-tipo').value = data.tipoPresupuesto || 'Gasto Fijo';
    document.getElementById('pres-monto').value = data.montoPresupuestado ?? '';
    document.getElementById('pres-cuenta-puc').value = data.cuentaContableId || '';
    document.getElementById('pres-categoria').value = data.categoriaId || '';
    document.getElementById('pres-descripcion').value = data.descripcion || '';

  } else {
    // Modo creación
    form.reset();

    title.innerHTML = '<i class="bi bi-plus-circle"></i> Nuevo Presupuesto Financiero';

    document.getElementById('pres-id').value = '';

    // Cargar los períodos disponibles
    poblarSelectoresPeriodo();
  }

  // Abrir el modal
  modal.classList.add('open');
}


// Abrir modal para crear presupuesto
function abrirModalPresupuesto() {
  toggleFormPresupuesto();
}


// Cerrar modal de presupuesto
function cerrarModalPresupuesto() {
  const modal = document.getElementById('modal-presupuesto');

  if (modal) {
    modal.classList.remove('open');
  }
}

function editarPresupuesto(id) {
  const p = AppState.presupuestos.find(item => item.id === id);
  if (p) toggleFormPresupuesto(p);
}

async function guardarPresupuesto(e) {
  e.preventDefault();
  if (!AppState.empresaActivaId) {
    showToast('Seleccione una empresa activa', 'error');
    return;
  }

  const id = document.getElementById('pres-id').value;
  const codigo = document.getElementById('pres-codigo').value.trim();
  const nombre = document.getElementById('pres-nombre').value.trim();
  const categoriaSeccion = document.getElementById('pres-categoria-seccion').value;
  const periodoMes = parseInt(document.getElementById('pres-mes').value);
  const periodoAnio = parseInt(document.getElementById('pres-anio').value);
  const tipoPresupuesto = document.getElementById('pres-tipo').value;
  const montoPresupuestado = parseFloat(document.getElementById('pres-monto').value);
  const cuentaContableId = document.getElementById('pres-cuenta-puc').value ? parseInt(document.getElementById('pres-cuenta-puc').value) : null;
  const categoriaId = parseInt(document.getElementById('pres-categoria').value);
  const descripcion = document.getElementById('pres-descripcion').value.trim() || null;

  if (montoPresupuestado < 0) {
    showToast('El monto presupuestado no puede ser negativo', 'error');
    return;
  }

  const payload = {
    periodoMes,
    periodoAnio,
    categoriaId,
    montoPresupuestado,
    codigo,
    nombre,
    descripcion,
    cuentaContableId,
    tipoPresupuesto,
    categoriaNombre: categoriaSeccion,
    estado: 'Activo'
  };

  let res;
  if (id) {
    res = await apiRequest(`/empresas/${AppState.empresaActivaId}/presupuestos/${id}`, 'PUT', payload);
  } else {
    res = await apiRequest(`/empresas/${AppState.empresaActivaId}/presupuestos`, 'POST', payload);
  }

  if (res) {
    showToast(id ? 'Presupuesto actualizado' : 'Presupuesto creado con éxito', 'success');
    cerrarModalPresupuesto();
    await loadPresupuestos();
    await loadDashboard();
  }
}

async function eliminarPresupuesto(id) {
  if (!confirm('¿Eliminar esta asignación de presupuesto?')) return;
  const res = await apiRequest(`/empresas/${AppState.empresaActivaId}/presupuestos/${id}`, 'DELETE');
  if (res) {
    showToast('Presupuesto eliminado', 'success');
    await loadPresupuestos();
    await loadDashboard();
  }
}

// ═══════════════════════════════════════════════════════════════
// 7. MÓDULO PLAN ÚNICO DE CUENTAS (PUC COLOMBIA)
// ═══════════════════════════════════════════════════════════════
async function cargarCuentasContablesEmpresa() {
  if (!AppState.empresaActivaId) return;
  const data = await apiRequest(`/empresas/${AppState.empresaActivaId}/cuentas`);
  AppState.cuentasContables = data || [];
  actualizarSelectoresCuentas(AppState.cuentasContables);
}

function actualizarSelectoresCuentas(cuentas) {
  const txSelect = document.getElementById('tx-cuenta-select');
  const presSelect = document.getElementById('pres-cuenta-puc');

  if (txSelect) {
    txSelect.innerHTML = '<option value="">— Seleccione cuenta contable PUC —</option>' +
      cuentas.map(c => `
        <option value="${c.id}" ${!c.manejaMovimientos ? 'disabled style="color:var(--text-muted); font-weight:bold"' : ''}>
          ${c.codigo} — ${c.nombre} (${c.naturaleza}${!c.manejaMovimientos ? ' - Agrupación' : ''})
        </option>
      `).join('');
  }

  if (presSelect) {
    presSelect.innerHTML = '<option value="">— Opcional: Vincular a cuenta contable PUC —</option>' +
      cuentas.map(c => `<option value="${c.id}">${c.codigo} — ${c.nombre}</option>`).join('');
  }
}

async function loadCuentasContables() {
  if (!AppState.empresaActivaId) return;

  const naturaleza = document.getElementById('puc-filter-naturaleza').value;
  const estado = document.getElementById('puc-filter-estado').value;

  let url = `/empresas/${AppState.empresaActivaId}/cuentas?`;
  if (AppState.filtroClasePuc) url += `clase=${AppState.filtroClasePuc}&`;
  if (naturaleza) url += `naturaleza=${naturaleza}&`;
  if (estado) url += `estado=${estado}&`;

  const data = await apiRequest(url);
  AppState.cuentasContables = data || [];
  renderCuentasTable(AppState.cuentasContables);
}

function renderCuentasTable(items) {
  const tbody = document.getElementById('cuentas-body');
  if (!tbody) return;

  if (!items || items.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="9" class="empty-state" style="padding:2.5rem">
          No hay cuentas contables registradas para este filtro.<br>
          <button class="btn btn-outline btn-sm" style="margin-top:0.75rem" onclick="sembrarPucColombiano()">
            <i class="bi bi-cloud-arrow-down"></i> Cargar PUC Base Colombiano
          </button>
        </td>
      </tr>`;
    return;
  }

  tbody.innerHTML = items.map(c => {
    const claseBadge = `<span class="badge-code">Clase ${c.clase}</span>`;
    const naturalezaBadge = `<span class="badge-${c.naturaleza.toLowerCase()}">${c.naturaleza}</span>`;
    const movBadge = c.manejaMovimientos
      ? '<span class="badge-mov si">Movimiento</span>'
      : '<span class="badge-mov no">Agrupación</span>';
    const terceroBadge = c.manejaTerceros
      ? '<span class="badge-tercero">Sí</span>'
      : '<span style="color:var(--text-muted)">No</span>';
    const estadoBadge = c.estado === 'Activa'
      ? '<span class="alert-badge verde">Activa</span>'
      : '<span class="alert-badge rojo">Inactiva</span>';

    return `
      <tr>
        <td><span class="badge-code" style="font-size:0.95rem">${c.codigo}</span></td>
        <td>
          <b>${c.nombre}</b>
          ${c.descripcion ? `<br><small style="color:var(--text-secondary)">${c.descripcion}</small>` : ''}
        </td>
        <td>${claseBadge} <small style="color:var(--text-muted)">(${c.nivel})</small></td>
        <td><span class="alert-badge verde">${c.tipoCategoria}</span></td>
        <td>${naturalezaBadge}</td>
        <td>${terceroBadge}</td>
        <td>${movBadge}</td>
        <td>${estadoBadge}</td>
        <td>
          <div class="btn-group">
            <button class="btn btn-secondary btn-sm" onclick="editarCuenta(${c.id})" title="Editar cuenta">
              <i class="bi bi-pencil"></i>
            </button>
            <button class="btn btn-secondary btn-sm" onclick="toggleEstadoCuenta(${c.id})" title="Alternar Activa/Inactiva">
              <i class="bi bi-toggle-${c.estado === 'Activa' ? 'on' : 'off'}"></i>
            </button>
            <button class="btn btn-danger btn-sm" onclick="eliminarCuenta(${c.id})" title="Eliminar cuenta">
              <i class="bi bi-trash"></i>
            </button>
          </div>
        </td>
      </tr>
    `;
  }).join('');
}

function filtrarCuentasPorClase(clase) {
  AppState.filtroClasePuc = clase;
  document.querySelectorAll('#puc-clase-pills .filter-pill').forEach((pill, idx) => {
    pill.classList.toggle('active', (clase === null && idx === 0) || (clase !== null && pill.textContent.startsWith(String(clase))));
  });
  loadCuentasContables();
}

function filtrarCuentas() {
  const query = (document.getElementById('puc-search-input').value || '').trim().toLowerCase();
  if (!query) {
    renderCuentasTable(AppState.cuentasContables);
    return;
  }

  const filtradas = AppState.cuentasContables.filter(c =>
    c.codigo.toLowerCase().includes(query) ||
    c.nombre.toLowerCase().includes(query) ||
    (c.descripcion && c.descripcion.toLowerCase().includes(query)) ||
    c.tipoCategoria.toLowerCase().includes(query)
  );

  renderCuentasTable(filtradas);
}

function limpiarFiltrosCuentas() {
  document.getElementById('puc-search-input').value = '';
  document.getElementById('puc-filter-naturaleza').value = '';
  document.getElementById('puc-filter-estado').value = '';
  filtrarCuentasPorClase(null);
}

async function sembrarPucColombiano() {
  if (!AppState.empresaActivaId) {
    showToast('Seleccione una empresa activa', 'error');
    return;
  }

  if (!confirm('¿Desea cargar el catálogo base oficial del PUC Comercial colombiano para esta empresa?')) return;

  const res = await apiRequest(`/empresas/${AppState.empresaActivaId}/cuentas/seed-puc`, 'POST');
  if (res) {
    showToast(res.message, 'success');
    await loadCuentasContables();
    await cargarCuentasContablesEmpresa();
  }
}

function abrirModalCuenta(cuenta = null) {
  const modal = document.getElementById('modal-cuenta');
  const title = document.getElementById('modal-cuenta-title');
  const form = document.getElementById('form-cuenta-modal');

  if (cuenta) {
    title.innerHTML = '<i class="bi bi-pencil-square"></i> Editar Cuenta Contable';
    document.getElementById('modal-cuenta-id').value = cuenta.id;
    document.getElementById('m-cuenta-codigo').value = cuenta.codigo;
    document.getElementById('m-cuenta-nombre').value = cuenta.nombre;
    document.getElementById('m-cuenta-clase').value = cuenta.clase;
    document.getElementById('m-cuenta-naturaleza').value = cuenta.naturaleza;
    document.getElementById('m-cuenta-nivel').value = cuenta.nivel;
    document.getElementById('m-cuenta-categoria').value = cuenta.tipoCategoria;
    document.getElementById('m-cuenta-terceros').value = String(cuenta.manejaTerceros);
    document.getElementById('m-cuenta-movimientos').value = String(cuenta.manejaMovimientos);
    document.getElementById('m-cuenta-estado').value = cuenta.estado;
    document.getElementById('m-cuenta-desc').value = cuenta.descripcion || '';
  } else {
    title.innerHTML = '<i class="bi bi-journal-plus"></i> Nueva Cuenta Contable PUC';
    form.reset();
    document.getElementById('modal-cuenta-id').value = '';
  }

  modal.classList.add('open');
}

function cerrarModalCuenta() {
  document.getElementById('modal-cuenta').classList.remove('open');
}

function onCodigoCuentaModalInput() {
  const codigo = document.getElementById('m-cuenta-codigo').value.trim();
  if (!codigo) return;

  // Autodetectar clase según primer dígito PUC
  const primerDigito = parseInt(codigo.charAt(0), 10);
  if (primerDigito >= 1 && primerDigito <= 7) {
    document.getElementById('m-cuenta-clase').value = primerDigito;
    onClaseCuentaModalChange();
  }

  // Autodetectar nivel según longitud de dígitos
  const nivelSelect = document.getElementById('m-cuenta-nivel');
  if (codigo.length === 1) nivelSelect.value = 'Clase';
  else if (codigo.length === 2) nivelSelect.value = 'Grupo';
  else if (codigo.length === 4) nivelSelect.value = 'Cuenta';
  else if (codigo.length === 6) nivelSelect.value = 'Subcuenta';
  else if (codigo.length >= 8) nivelSelect.value = 'Auxiliar';
}

function onClaseCuentaModalChange() {
  const clase = parseInt(document.getElementById('m-cuenta-clase').value, 10);
  const naturalezaSelect = document.getElementById('m-cuenta-naturaleza');
  const catSelect = document.getElementById('m-cuenta-categoria');

  // Reglas contables colombianas PUC:
  // Activos (1), Gastos (5), Costos (6, 7) => Débito
  // Pasivos (2), Patrimonio (3), Ingresos (4) => Crédito
  if (clase === 1 || clase === 5 || clase === 6 || clase === 7) {
    naturalezaSelect.value = 'Debito';
  } else {
    naturalezaSelect.value = 'Credito';
  }

  const mapaCat = {
    1: 'Activo',
    2: 'Pasivo',
    3: 'Patrimonio',
    4: 'Ingreso',
    5: 'Gasto',
    6: 'Costo',
    7: 'Costo'
  };
  if (mapaCat[clase]) catSelect.value = mapaCat[clase];
}

function editarCuenta(id) {
  const c = AppState.cuentasContables.find(item => item.id === id);
  if (c) abrirModalCuenta(c);
}

async function guardarCuentaContable(e) {
  e.preventDefault();
  if (!AppState.empresaActivaId) {
    showToast('Seleccione una empresa activa', 'error');
    return;
  }

  const id = document.getElementById('modal-cuenta-id').value;
  const codigo = document.getElementById('m-cuenta-codigo').value.trim();
  const nombre = document.getElementById('m-cuenta-nombre').value.trim();
  const clase = parseInt(document.getElementById('m-cuenta-clase').value, 10);
  const naturaleza = document.getElementById('m-cuenta-naturaleza').value;
  const nivel = document.getElementById('m-cuenta-nivel').value;
  const tipoCategoria = document.getElementById('m-cuenta-categoria').value;
  const manejaTerceros = document.getElementById('m-cuenta-terceros').value === 'true';
  const manejaMovimientos = document.getElementById('m-cuenta-movimientos').value === 'true';
  const estado = document.getElementById('m-cuenta-estado').value;
  const descripcion = document.getElementById('m-cuenta-desc').value.trim() || null;

  const payload = {
    codigo,
    nombre,
    descripcion,
    clase,
    grupo: codigo.length >= 2 ? codigo.substring(0, 2) : null,
    nivel,
    naturaleza,
    tipoCategoria,
    categoriaId: null,
    manejaTerceros,
    manejaMovimientos,
    cuentaPadreCodigo: null,
    estado
  };

  let res;
  if (id) {
    res = await apiRequest(`/empresas/${AppState.empresaActivaId}/cuentas/${id}`, 'PUT', payload);
  } else {
    res = await apiRequest(`/empresas/${AppState.empresaActivaId}/cuentas`, 'POST', payload);
  }

  if (res) {
    showToast(id ? 'Cuenta contable actualizada' : 'Cuenta contable creada con éxito', 'success');
    cerrarModalCuenta();
    await loadCuentasContables();
    await cargarCuentasContablesEmpresa();
  }
}

async function toggleEstadoCuenta(id) {
  const res = await apiRequest(`/empresas/${AppState.empresaActivaId}/cuentas/${id}/toggle-estado`, 'PATCH');
  if (res) {
    showToast('Estado de cuenta actualizado', 'info');
    await loadCuentasContables();
    await cargarCuentasContablesEmpresa();
  }
}

async function eliminarCuenta(id) {
  if (!confirm('¿Confirma la eliminación de esta cuenta contable? Solo procederá si no tiene transacciones asociadas.')) return;
  const res = await apiRequest(`/empresas/${AppState.empresaActivaId}/cuentas/${id}`, 'DELETE');
  if (res) {
    showToast('Cuenta contable eliminada', 'success');
    await loadCuentasContables();
    await cargarCuentasContablesEmpresa();
  }
}

// ═══════════════════════════════════════════════════════════════
// 8. MÓDULO DE TERCEROS (CLIENTES, PROVEEDORES, EMPLEADOS)
// ═══════════════════════════════════════════════════════════════
async function cargarTercerosEmpresa() {
  if (!AppState.empresaActivaId) return;
  const data = await apiRequest(`/empresas/${AppState.empresaActivaId}/terceros`);
  AppState.terceros = data || [];
  actualizarSelectoresTerceros(AppState.terceros);
}

function actualizarSelectoresTerceros(terceros) {
  const select = document.getElementById('tx-tercero-select');
  if (!select) return;
  select.innerHTML = '<option value="">— Seleccionar tercero existente —</option>' +
    terceros.map(t => `
      <option value="${t.id}">${t.razonSocial} (${t.tipoTercero} - NIT: ${t.numeroDocumento}${t.digitoVerificacion ? `-${t.digitoVerificacion}` : ''})</option>
    `).join('');
}

async function loadTerceros() {
  if (!AppState.empresaActivaId) return;

  const tipo = document.getElementById('terceros-filter-tipo').value;
  let url = `/empresas/${AppState.empresaActivaId}/terceros?`;
  if (tipo) url += `tipoTercero=${encodeURIComponent(tipo)}`;

  const data = await apiRequest(url);
  AppState.terceros = data || [];
  renderTercerosTable(AppState.terceros);
}

function renderTercerosTable(items) {
  const tbody = document.getElementById('terceros-body');
  if (!tbody) return;

  if (!items || items.length === 0) {
    tbody.innerHTML = '<tr><td colspan="7" class="empty-state" style="padding:2.5rem">Sin terceros registrados para esta empresa</td></tr>';
    return;
  }

  tbody.innerHTML = items.map(t => {
    const docDisplay = `<span class="badge-code">${t.tipoDocumento}: ${t.numeroDocumento}${t.digitoVerificacion ? `-${t.digitoVerificacion}` : ''}</span>`;
    const tipoBadge = `<span class="alert-badge verde">${t.tipoTercero}</span>`;
    const estadoBadge = t.estado === 'Activo'
      ? '<span class="alert-badge verde">Activo</span>'
      : '<span class="alert-badge rojo">Inactivo</span>';

    return `
      <tr>
        <td>${docDisplay}</td>
        <td>
          <b>${t.razonSocial}</b>
          ${t.nombreComercial ? `<br><small style="color:var(--text-secondary)">Sigla: ${t.nombreComercial}</small>` : ''}
        </td>
        <td>${tipoBadge}</td>
        <td>
          ${t.email ? `<small><i class="bi bi-envelope"></i> ${t.email}</small><br>` : ''}
          ${t.celular || t.telefono ? `<small><i class="bi bi-telephone"></i> ${t.celular || t.telefono}</small>` : '<span style="color:var(--text-muted)">—</span>'}
        </td>
        <td>
          ${t.municipio || t.departamento ? `${t.municipio || ''}, ${t.departamento || ''}` : '<span style="color:var(--text-muted)">—</span>'}
          ${t.direccion ? `<br><small style="color:var(--text-muted)">${t.direccion}</small>` : ''}
        </td>
        <td>${estadoBadge}</td>
        <td>
          <div class="btn-group">
            <button class="btn btn-secondary btn-sm" onclick="editarTercero(${t.id})" title="Editar tercero">
              <i class="bi bi-pencil"></i>
            </button>
            <button class="btn btn-danger btn-sm" onclick="eliminarTercero(${t.id})" title="Eliminar tercero">
              <i class="bi bi-trash"></i>
            </button>
          </div>
        </td>
      </tr>
    `;
  }).join('');
}

function filtrarTerceros() {
  const query = (document.getElementById('terceros-search-input').value || '').trim().toLowerCase();
  if (!query) {
    renderTercerosTable(AppState.terceros);
    return;
  }

  const filtrados = AppState.terceros.filter(t =>
    t.numeroDocumento.toLowerCase().includes(query) ||
    t.razonSocial.toLowerCase().includes(query) ||
    (t.nombreComercial && t.nombreComercial.toLowerCase().includes(query)) ||
    (t.email && t.email.toLowerCase().includes(query))
  );

  renderTercerosTable(filtrados);
}

function limpiarFiltrosTerceros() {
  document.getElementById('terceros-search-input').value = '';
  document.getElementById('terceros-filter-tipo').value = '';
  loadTerceros();
}

function abrirModalTercero(t = null) {
  const modal = document.getElementById('modal-tercero');
  const title = document.getElementById('modal-tercero-title');
  const form = document.getElementById('form-tercero-modal');

  if (t) {
    title.innerHTML = '<i class="bi bi-pencil-square"></i> Editar Tercero';
    document.getElementById('modal-tercero-id').value = t.id;
    document.getElementById('m-ter-tipo').value = t.tipoTercero;
    document.getElementById('m-ter-tipodoc').value = t.tipoDocumento;
    document.getElementById('m-ter-doc').value = t.numeroDocumento;
    document.getElementById('m-ter-dv').value = t.digitoVerificacion || '';
    document.getElementById('m-ter-razon').value = t.razonSocial;
    document.getElementById('m-ter-comercial').value = t.nombreComercial || '';
    document.getElementById('m-ter-depto').value = t.departamento || '';
    document.getElementById('m-ter-muni').value = t.municipio || '';
    document.getElementById('m-ter-dir').value = t.direccion || '';
    document.getElementById('m-ter-email').value = t.email || '';
    document.getElementById('m-ter-tel').value = t.celular || t.telefono || '';
    document.getElementById('m-ter-estado').value = t.estado || 'Activo';
    document.getElementById('m-ter-obs').value = t.observaciones || '';
  } else {
    title.innerHTML = '<i class="bi bi-person-plus"></i> Registro de Tercero';
    form.reset();
    document.getElementById('modal-tercero-id').value = '';
  }

  modal.classList.add('open');
}

function cerrarModalTercero() {
  document.getElementById('modal-tercero').classList.remove('open');
}

function onModalTerceroDocChange() {
  onModalTerceroDocInput();
}

function onModalTerceroDocInput() {
  const tipoDoc = document.getElementById('m-ter-tipodoc').value;
  const doc = document.getElementById('m-ter-doc').value;
  const dvInput = document.getElementById('m-ter-dv');
  if (tipoDoc === 'NIT' && doc) {
    dvInput.value = calcularDv(doc);
  } else {
    dvInput.value = '';
  }
}

function editarTercero(id) {
  const t = AppState.terceros.find(item => item.id === id);
  if (t) abrirModalTercero(t);
}

async function guardarTercero(e) {
  e.preventDefault();
  if (!AppState.empresaActivaId) {
    showToast('Seleccione una empresa activa', 'error');
    return;
  }

  const id = document.getElementById('modal-tercero-id').value;
  const tipoTercero = document.getElementById('m-ter-tipo').value;
  const tipoDocumento = document.getElementById('m-ter-tipodoc').value;
  const numeroDocumento = document.getElementById('m-ter-doc').value.trim();
  const digitoVerificacion = document.getElementById('m-ter-dv').value.trim() || null;
  const razonSocial = document.getElementById('m-ter-razon').value.trim();
  const nombreComercial = document.getElementById('m-ter-comercial').value.trim() || null;
  const departamento = document.getElementById('m-ter-depto').value.trim() || null;
  const municipio = document.getElementById('m-ter-muni').value.trim() || null;
  const direccion = document.getElementById('m-ter-dir').value.trim() || null;
  const email = document.getElementById('m-ter-email').value.trim() || null;
  const celular = document.getElementById('m-ter-tel').value.trim() || null;
  const estado = document.getElementById('m-ter-estado').value;
  const observaciones = document.getElementById('m-ter-obs').value.trim() || null;

  const payload = {
    tipoTercero,
    tipoDocumento,
    numeroDocumento,
    digitoVerificacion,
    razonSocial,
    nombreComercial,
    primerNombre: null,
    primerApellido: null,
    departamento,
    municipio,
    direccion,
    email,
    telefono: celular,
    celular,
    estado,
    observaciones
  };

  let res;
  if (id) {
    res = await apiRequest(`/empresas/${AppState.empresaActivaId}/terceros/${id}`, 'PUT', payload);
  } else {
    res = await apiRequest(`/empresas/${AppState.empresaActivaId}/terceros`, 'POST', payload);
  }

  if (res) {
    showToast(id ? 'Tercero actualizado' : 'Tercero registrado con éxito', 'success');
    cerrarModalTercero();
    await loadTerceros();
    await cargarTercerosEmpresa();
  }
}

async function eliminarTercero(id) {
  if (!confirm('¿Confirma la eliminación del tercero? Solo se eliminará si no posee movimientos contables.')) return;
  const res = await apiRequest(`/empresas/${AppState.empresaActivaId}/terceros/${id}`, 'DELETE');
  if (res) {
    showToast('Tercero eliminado', 'success');
    await loadTerceros();
    await cargarTercerosEmpresa();
  }
}

// ═══════════════════════════════════════════════════════════════
// 9. MÓDULO DE CENTROS DE COSTO
// ═══════════════════════════════════════════════════════════════
async function cargarCentrosCostoEmpresa() {
  if (!AppState.empresaActivaId) return;
  const data = await apiRequest(`/empresas/${AppState.empresaActivaId}/centros-costo`);
  AppState.centrosCosto = data || [];
  actualizarSelectoresCentrosCosto(AppState.centrosCosto);
}

function actualizarSelectoresCentrosCosto(centros) {
  const select = document.getElementById('tx-centro-costo');
  if (!select) return;
  select.innerHTML = '<option value="">— Sin centro de costo —</option>' +
    centros.map(cc => `<option value="${cc.id}">${cc.codigo} — ${cc.nombre}</option>`).join('');
}

async function loadCentrosCosto() {
  if (!AppState.empresaActivaId) return;
  const data = await apiRequest(`/empresas/${AppState.empresaActivaId}/centros-costo`);
  AppState.centrosCosto = data || [];
  renderCentrosCostoTable(AppState.centrosCosto);
}

function renderCentrosCostoTable(items) {
  const tbody = document.getElementById('centros-costo-body');
  if (!tbody) return;

  if (!items || items.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="5" class="empty-state" style="padding:2.5rem">
          Sin centros de costo registrados.<br>
          <button class="btn btn-outline btn-sm" style="margin-top:0.75rem" onclick="sembrarCentrosCostoBase()">
            <i class="bi bi-magic"></i> Cargar Centros Base
          </button>
        </td>
      </tr>`;
    return;
  }

  tbody.innerHTML = items.map(cc => `
    <tr>
      <td><span class="badge-code">${cc.codigo}</span></td>
      <td><b>${cc.nombre}</b></td>
      <td>${cc.descripcion || '<span style="color:var(--text-muted)">Sin descripción</span>'}</td>
      <td><span class="alert-badge verde">${cc.estado}</span></td>
      <td>
        <div class="btn-group">
          <button class="btn btn-secondary btn-sm" onclick="editarCentroCosto(${cc.id})" title="Editar centro">
            <i class="bi bi-pencil"></i>
          </button>
          <button class="btn btn-danger btn-sm" onclick="eliminarCentroCosto(${cc.id})" title="Eliminar centro">
            <i class="bi bi-trash"></i>
          </button>
        </div>
      </td>
    </tr>
  `).join('');
}

async function sembrarCentrosCostoBase() {
  if (!AppState.empresaActivaId) {
    showToast('Seleccione una empresa activa', 'error');
    return;
  }

  const res = await apiRequest(`/empresas/${AppState.empresaActivaId}/centros-costo/seed`, 'POST');
  if (res) {
    showToast(res.message, 'success');
    await loadCentrosCosto();
    await cargarCentrosCostoEmpresa();
  }
}

function abrirModalCentroCosto(cc = null) {
  const modal = document.getElementById('modal-centro-costo');
  const title = document.getElementById('modal-cc-title');
  const form = document.getElementById('form-cc-modal');

  if (cc) {
    title.innerHTML = '<i class="bi bi-pencil-square"></i> Editar Centro de Costo';
    document.getElementById('modal-cc-id').value = cc.id;
    document.getElementById('m-cc-codigo').value = cc.codigo;
    document.getElementById('m-cc-nombre').value = cc.nombre;
    document.getElementById('m-cc-desc').value = cc.descripcion || '';
    document.getElementById('m-cc-estado').value = cc.estado;
  } else {
    title.innerHTML = '<i class="bi bi-diagram-3"></i> Nuevo Centro de Costo';
    form.reset();
    document.getElementById('modal-cc-id').value = '';
  }

  modal.classList.add('open');
}

function cerrarModalCentroCosto() {
  document.getElementById('modal-centro-costo').classList.remove('open');
}

function editarCentroCosto(id) {
  const cc = AppState.centrosCosto.find(item => item.id === id);
  if (cc) abrirModalCentroCosto(cc);
}

async function guardarCentroCosto(e) {
  e.preventDefault();
  if (!AppState.empresaActivaId) {
    showToast('Seleccione una empresa activa', 'error');
    return;
  }

  const id = document.getElementById('modal-cc-id').value;
  const codigo = document.getElementById('m-cc-codigo').value.trim();
  const nombre = document.getElementById('m-cc-nombre').value.trim();
  const descripcion = document.getElementById('m-cc-desc').value.trim() || null;
  const estado = document.getElementById('m-cc-estado').value;

  const payload = { codigo, nombre, descripcion, estado };

  let res;
  if (id) {
    res = await apiRequest(`/empresas/${AppState.empresaActivaId}/centros-costo/${id}`, 'PUT', payload);
  } else {
    res = await apiRequest(`/empresas/${AppState.empresaActivaId}/centros-costo`, 'POST', payload);
  }

  if (res) {
    showToast(id ? 'Centro de costo actualizado' : 'Centro de costo creado con éxito', 'success');
    cerrarModalCentroCosto();
    await loadCentrosCosto();
    await cargarCentrosCostoEmpresa();
  }
}

async function eliminarCentroCosto(id) {
  if (!confirm('¿Confirma la eliminación del centro de costo?')) return;
  const res = await apiRequest(`/empresas/${AppState.empresaActivaId}/centros-costo/${id}`, 'DELETE');
  if (res) {
    showToast('Centro de costo eliminado', 'success');
    await loadCentrosCosto();
    await cargarCentrosCostoEmpresa();
  }
}

// ═══════════════════════════════════════════════════════════════
// 10. MÓDULO DE GESTIÓN INTEGRAL DE EMPRESAS Y CLIENTES
// ═══════════════════════════════════════════════════════════════
function renderEmpresasCards() {
  const grid = document.getElementById('empresas-grid');
  if (!grid) return;

  if (!AppState.empresas || AppState.empresas.length === 0) {
    grid.innerHTML = `
      <div class="empty-state" style="grid-column: 1 / -1">
        <div class="empty-icon"><i class="bi bi-building"></i></div>
        <p>No tiene empresas registradas. Utilice el botón superior para registrar sus clientes.</p>
      </div>`;
    return;
  }

  grid.innerHTML = AppState.empresas.map(emp => {
    const isActiva = emp.id == AppState.empresaActivaId;
    const docDisplay = emp.nit ? `NIT: ${emp.nit}` : (emp.numeroDocumento ? `Doc: ${emp.numeroDocumento}` : 'Sin NIT registrado');
    const estadoClass = emp.estado === 'Activa' ? 'verde' : 'rojo';

    return `
      <div class="empresa-card ${isActiva ? 'active' : ''}">
        <div style="display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:0.5rem">
          <div class="emp-name">${emp.nombreEmpresa}</div>
          <span class="alert-badge ${estadoClass}">${emp.estado || 'Activa'}</span>
        </div>
        <div class="emp-nit">
          <i class="bi bi-card-text"></i> ${docDisplay}
          ${emp.tipoPersona ? ` &bull; <small style="color:var(--primary-300)">${emp.tipoPersona}</small>` : ''}
        </div>
        ${emp.sector ? `<span class="emp-sector">${emp.sector}</span>` : ''}
        
        <p style="font-size:0.83rem; color:var(--text-secondary); margin:0.75rem 0">
          ${emp.descripcion || 'Sin descripción adicional registrada.'}
        </p>

        <div style="font-size:0.78rem; color:var(--text-muted); margin-bottom:1rem">
          ${emp.municipio || emp.departamento ? `<div><i class="bi bi-geo-alt"></i> ${emp.municipio || ''}, ${emp.departamento || ''}</div>` : ''}
          ${emp.responsableNombre ? `<div><i class="bi bi-person"></i> Resp: ${emp.responsableNombre} (${emp.responsableRol || 'Contacto'})</div>` : ''}
          ${emp.regimenTributario ? `<div><i class="bi bi-shield-check"></i> ${emp.regimenTributario}</div>` : ''}
        </div>

        <div class="emp-actions">
          <button class="btn btn-primary btn-sm" onclick="seleccionarEmpresa(${emp.id})">
            ${isActiva ? '<i class="bi bi-check2"></i> Entorno Activo' : '<i class="bi bi-box-arrow-in-right"></i> Seleccionar'}
          </button>
          <button class="btn btn-secondary btn-sm" onclick="abrirFormularioEmpresa(${emp.id})" title="Editar empresa">
            <i class="bi bi-pencil"></i>
          </button>
          <button class="btn btn-danger btn-sm" onclick="eliminarEmpresa(${emp.id})" title="Eliminar empresa">
            <i class="bi bi-trash"></i>
          </button>
        </div>
      </div>
    `;
  }).join('');
}

async function seleccionarEmpresa(id) {
  AppState.empresaActivaId = id;
  localStorage.setItem('dss_empresa_activa_id', id);
  document.getElementById('empresa-activa').value = id;
  await onEmpresaCambiada();
  navigateTo('dashboard');
  showToast('Empresa seleccionada como entorno activo de trabajo', 'info');
}

function switchEmpTab(tabName) {
  document.querySelectorAll('.form-tab-btn').forEach(btn => {
    btn.classList.toggle('active', btn.getAttribute('onclick').includes(tabName));
  });

  document.querySelectorAll('.form-tab-pane').forEach(pane => {
    pane.classList.toggle('active', pane.id === `emp-tab-${tabName}`);
  });
}

function onTipoPersonaChange() {
  const tipo = document.getElementById('emp-tipo-persona').value;
  const isJuridica = tipo === 'Juridica';
  document.getElementById('emp-fields-juridica').style.display = isJuridica ? 'block' : 'none';
  document.getElementById('emp-fields-natural').style.display = isJuridica ? 'none' : 'block';

  const tipoDocSelect = document.getElementById('emp-tipo-doc');
  if (isJuridica) {
    tipoDocSelect.value = 'NIT';
  } else if (tipoDocSelect.value === 'NIT') {
    tipoDocSelect.value = 'CC';
  }

  onTipoDocumentoChange();
}

function onTipoDocumentoChange() {
  const tipoDoc = document.getElementById('emp-tipo-doc').value;
  const dvContainer = document.getElementById('emp-dv').parentElement.parentElement;
  if (tipoDoc === 'NIT') {
    dvContainer.style.display = 'block';
    onNumeroDocumentoInput();
  } else {
    dvContainer.style.display = 'none';
    document.getElementById('emp-dv').value = '';
  }
}

function onNumeroDocumentoInput() {
  const tipoDoc = document.getElementById('emp-tipo-doc').value;
  const num = document.getElementById('emp-doc-num').value.trim();
  const dvInput = document.getElementById('emp-dv');
  if (tipoDoc === 'NIT' && num) {
    dvInput.value = calcularDv(num);
  } else {
    dvInput.value = '';
  }
}

function recalcularDv() {
  const num = document.getElementById('emp-doc-num').value.trim();
  if (num) {
    document.getElementById('emp-dv').value = calcularDv(num);
    showToast('Dígito de verificación calculado', 'info');
  }
}

function onDepartamentoChange() {
  // Hook para autocompletar o sugerir municipios
}

function abrirFormularioEmpresa(empresaId = null) {
  const modal = document.getElementById('modal-empresa');
  const form = document.getElementById('form-empresa-completo');
  const title = document.getElementById('emp-form-title');

  if (!modal || !form || !title) {
    console.error('No se encontró el modal de empresas o alguno de sus elementos.');
    return;
  }

  // Mostrar la pestaña inicial
  switchEmpTab('identificacion');

  if (empresaId) {
    const emp = AppState.empresas.find(e => e.id === empresaId);

    if (!emp) {
      console.error('No se encontró la empresa con ID:', empresaId);
      return;
    }

    title.innerHTML = '<i class="bi bi-pencil-square"></i> Editar Empresa / Persona Administrada';
    document.getElementById('emp-edit-id').value = emp.id;

    document.getElementById('emp-tipo-persona').value = emp.tipoPersona || 'Juridica';
    document.getElementById('emp-tipo-doc').value = emp.tipoDocumento || 'NIT';
    document.getElementById('emp-doc-num').value = emp.numeroDocumento || emp.nit || '';
    document.getElementById('emp-dv').value = emp.digitoVerificacion || '';
    document.getElementById('emp-razon-social').value = emp.razonSocial || emp.nombreEmpresa || '';
    document.getElementById('emp-nombre-comercial').value = emp.nombreComercial || '';
    document.getElementById('emp-primer-nombre').value = emp.primerNombre || '';
    document.getElementById('emp-segundo-nombre').value = emp.segundoNombre || '';
    document.getElementById('emp-primer-apellido').value = emp.primerApellido || '';
    document.getElementById('emp-segundo-apellido').value = emp.segundoApellido || '';

    document.getElementById('emp-nombre-resumen').value = emp.nombreEmpresa || '';
    document.getElementById('emp-sector').value = emp.sector || '';
    document.getElementById('emp-estado').value = emp.estado || 'Activa';
    document.getElementById('emp-desc').value = emp.descripcion || '';

    document.getElementById('emp-pais').value = emp.pais || 'Colombia';
    document.getElementById('emp-departamento').value = emp.departamento || '';
    document.getElementById('emp-municipio').value = emp.municipio || '';
    document.getElementById('emp-direccion').value = emp.direccionPrincipal || '';
    document.getElementById('emp-complemento').value = emp.complementoDireccion || '';
    document.getElementById('emp-barrio').value = emp.barrio || '';
    document.getElementById('emp-cp').value = emp.codigoPostal || '';
    document.getElementById('emp-email').value = emp.emailContacto || '';
    document.getElementById('emp-tel').value = emp.celular || emp.telefonoFijo || '';
    document.getElementById('emp-web').value = emp.sitioWeb || '';
    document.getElementById('emp-correspondencia').value = emp.direccionCorrespondencia || '';

    document.getElementById('emp-resp-nombre').value = emp.responsableNombre || '';
    document.getElementById('emp-resp-rol').value = emp.responsableRol || 'Representante Legal';
    document.getElementById('emp-resp-cargo').value = emp.responsableCargo || '';
    document.getElementById('emp-resp-doc').value = emp.responsableDocumento || '';
    document.getElementById('emp-resp-email').value = emp.responsableEmail || '';
    document.getElementById('emp-resp-tel').value = emp.responsableTelefono || '';
    document.getElementById('emp-resp-obs').value = emp.responsableObservaciones || '';

    document.getElementById('emp-regimen').value = emp.regimenTributario || 'Responsable de IVA';
    document.getElementById('emp-iva').value = emp.responsabilidadIva || 'Responsable';
    document.getElementById('emp-ciiu').value = emp.codigoCiiu || '';
    document.getElementById('emp-actividad').value = emp.actividadEconomica || '';
    document.getElementById('emp-secundarias').value = emp.actividadesSecundarias || '';
    document.getElementById('emp-responsabilidades').value = emp.responsabilidadesTributarias || '';
    document.getElementById('emp-obs-tributarias').value = emp.observacionesTributarias || '';

    onTipoPersonaChange();

  } else {
    title.innerHTML = '<i class="bi bi-building-add"></i> Registrar Nueva Empresa / Cliente';

    form.reset();

    document.getElementById('emp-edit-id').value = '';
    document.getElementById('emp-pais').value = 'Colombia';
    document.getElementById('emp-estado').value = 'Activa';

    onTipoPersonaChange();
  }

  // Abrir el modal
  modal.classList.add('open');
}

function cerrarFormularioEmpresa() {
  const modal = document.getElementById('modal-empresa');

  if (modal) {
    modal.classList.remove('open');
  }
}

function limpiarFormularioEmpresa() {
  document.getElementById('form-empresa-completo').reset();
  document.getElementById('emp-pais').value = 'Colombia';
  document.getElementById('emp-estado').value = 'Activa';
  onTipoPersonaChange();
}

async function guardarEmpresa(e) {
  e.preventDefault();

  const id = document.getElementById('emp-edit-id').value;
  const tipoPersona = document.getElementById('emp-tipo-persona').value;
  const tipoDocumento = document.getElementById('emp-tipo-doc').value;
  const numeroDocumento = document.getElementById('emp-doc-num').value.trim();
  const dv = document.getElementById('emp-dv').value.trim() || null;
  const razonSocial = document.getElementById('emp-razon-social').value.trim() || null;
  const nombreComercial = document.getElementById('emp-nombre-comercial').value.trim() || null;
  const primerNombre = document.getElementById('emp-primer-nombre').value.trim() || null;
  const segundoNombre = document.getElementById('emp-segundo-nombre').value.trim() || null;
  const primerApellido = document.getElementById('emp-primer-apellido').value.trim() || null;
  const segundoApellido = document.getElementById('emp-segundo-apellido').value.trim() || null;

  let nombreEmpresa = document.getElementById('emp-nombre-resumen').value.trim();
  if (!nombreEmpresa) {
    if (tipoPersona === 'Juridica' && razonSocial) {
      nombreEmpresa = razonSocial;
    } else if (tipoPersona === 'Natural' && (primerNombre || primerApellido)) {
      nombreEmpresa = `${primerNombre || ''} ${primerApellido || ''}`.trim();
    } else {
      nombreEmpresa = 'Empresa ' + numeroDocumento;
    }
  }

  const sector = document.getElementById('emp-sector').value.trim() || null;
  const estado = document.getElementById('emp-estado').value;
  const descripcion = document.getElementById('emp-desc').value.trim() || null;

  const pais = document.getElementById('emp-pais').value.trim() || 'Colombia';
  const departamento = document.getElementById('emp-departamento').value.trim() || null;
  const municipio = document.getElementById('emp-municipio').value.trim() || null;
  const direccionPrincipal = document.getElementById('emp-direccion').value.trim() || null;
  const complementoDireccion = document.getElementById('emp-complemento').value.trim() || null;
  const barrio = document.getElementById('emp-barrio').value.trim() || null;
  const codigoPostal = document.getElementById('emp-cp').value.trim() || null;
  const emailContacto = document.getElementById('emp-email').value.trim() || null;
  const celular = document.getElementById('emp-tel').value.trim() || null;
  const sitioWeb = document.getElementById('emp-web').value.trim() || null;
  const direccionCorrespondencia = document.getElementById('emp-correspondencia').value.trim() || null;

  const responsableNombre = document.getElementById('emp-resp-nombre').value.trim() || null;
  const responsableRol = document.getElementById('emp-resp-rol').value;
  const responsableCargo = document.getElementById('emp-resp-cargo').value.trim() || null;
  const responsableDocumento = document.getElementById('emp-resp-doc').value.trim() || null;
  const responsableEmail = document.getElementById('emp-resp-email').value.trim() || null;
  const responsableTelefono = document.getElementById('emp-resp-tel').value.trim() || null;
  const responsableObservaciones = document.getElementById('emp-resp-obs').value.trim() || null;

  const regimenTributario = document.getElementById('emp-regimen').value;
  const responsabilidadIva = document.getElementById('emp-iva').value;
  const codigoCiiu = document.getElementById('emp-ciiu').value.trim() || null;
  const actividadEconomica = document.getElementById('emp-actividad').value.trim() || null;
  const actividadesSecundarias = document.getElementById('emp-secundarias').value.trim() || null;
  const responsabilidadesTributarias = document.getElementById('emp-responsabilidades').value.trim() || null;
  const observacionesTributarias = document.getElementById('emp-obs-tributarias').value.trim() || null;

  const nitFormateado = (tipoDocumento === 'NIT' && dv) ? `${numeroDocumento}-${dv}` : numeroDocumento;

  const payload = {
    nombreEmpresa,
    nit: nitFormateado,
    sector,
    descripcion,
    tipoPersona,
    tipoDocumento,
    numeroDocumento,
    digitoVerificacion: dv,
    razonSocial,
    nombreComercial,
    primerNombre,
    segundoNombre,
    primerApellido,
    segundoApellido,
    estado,
    pais,
    departamento,
    municipio,
    direccionPrincipal,
    complementoDireccion,
    barrio,
    codigoPostal,
    emailContacto,
    telefonoFijo: celular,
    celular,
    sitioWeb,
    direccionCorrespondencia,
    responsableNombre,
    responsableTipoDocumento: 'CC',
    responsableDocumento,
    responsableCargo,
    responsableRol,
    responsableEmail,
    responsableTelefono,
    responsableObservaciones,
    regimenTributario,
    responsabilidadIva,
    codigoCiiu,
    actividadEconomica,
    actividadesSecundarias,
    responsabilidadesTributarias,
    observacionesTributarias
  };

  let res;
  if (id) {
    res = await apiRequest(`/empresas/${id}`, 'PUT', payload);
  } else {
    res = await apiRequest('/empresas', 'POST', payload);
  }

  if (res) {
    showToast(id ? 'Empresa actualizada exitosamente' : `Empresa '${nombreEmpresa}' registrada`, 'success');
    cerrarFormularioEmpresa();
    await cargarEmpresas();
    renderEmpresasCards();
  }
}

async function eliminarEmpresa(id) {
  if (!confirm('¿Confirma la eliminación definitiva de esta empresa? Se borrarán sus transacciones, presupuestos, cuentas e indicadores financieros.')) return;

  const res = await apiRequest(`/empresas/${id}`, 'DELETE');
  if (res) {
    showToast('Empresa eliminada', 'success');
    if (AppState.empresaActivaId == id) {
      AppState.empresaActivaId = null;
      localStorage.removeItem('dss_empresa_activa_id');
    }
    await cargarEmpresas();
    renderEmpresasCards();
  }
}

// ═══════════════════════════════════════════════════════════════
// 11. CATEGORÍAS MACRO DSS (COMPATIBILIDAD)
// ═══════════════════════════════════════════════════════════════
async function cargarCategoriasDss() {
  if (!AppState.empresaActivaId) return;
  const data = await apiRequest(`/empresas/${AppState.empresaActivaId}/categorias`);
  AppState.categoriasDss = data || [];

  const presCatSelect = document.getElementById('pres-categoria');
  if (presCatSelect) {
    presCatSelect.innerHTML = '<option value="">— Seleccione categoría macro —</option>' +
      AppState.categoriasDss.map(c => `<option value="${c.id}">${c.nombre} (${c.tipo})</option>`).join('');
  }
}
