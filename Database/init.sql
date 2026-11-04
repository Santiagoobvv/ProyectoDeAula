-- ═══════════════════════════════════════════════════════════════
-- JAS — PANEL DE CONTROL FINANCIERO Y CONTABLE (COLOMBIA)
-- Esquema completo de base de datos PostgreSQL
-- ═══════════════════════════════════════════════════════════════

-- 1. TABLA: usuarios (Contadores del sistema)
CREATE TABLE IF NOT EXISTS usuarios (
    id                SERIAL        PRIMARY KEY,
    nombre            VARCHAR(150)  NOT NULL,
    email             VARCHAR(255)  NOT NULL UNIQUE,
    password_hash     VARCHAR(512)  NOT NULL,
    fecha_creacion    TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_usuarios_email ON usuarios (email);

-- 2. TABLA: empresas (Empresas y Personas administradas por el contador)
CREATE TABLE IF NOT EXISTS empresas (
    id                            SERIAL        PRIMARY KEY,
    usuario_id                    INT           NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    nombre_empresa                VARCHAR(200)  NOT NULL,
    nit                           VARCHAR(50),
    sector                        VARCHAR(100),
    descripcion                   TEXT,
    fecha_creacion                TIMESTAMPTZ   NOT NULL DEFAULT NOW(),

    -- Campos ampliados de identificación
    tipo_persona                  VARCHAR(20)   NOT NULL DEFAULT 'Juridica'
                                  CHECK (tipo_persona IN ('Juridica', 'Natural')),
    tipo_documento                VARCHAR(30)   NOT NULL DEFAULT 'NIT',
    numero_documento              VARCHAR(50),
    digito_verificacion           VARCHAR(2),
    razon_social                  VARCHAR(250),
    nombre_comercial              VARCHAR(200),
    primer_nombre                 VARCHAR(100),
    segundo_nombre                VARCHAR(100),
    primer_apellido               VARCHAR(100),
    segundo_apellido              VARCHAR(100),
    estado                        VARCHAR(20)   NOT NULL DEFAULT 'Activa'
                                  CHECK (estado IN ('Activa', 'Inactiva')),

    -- Ubicación y contacto
    pais                          VARCHAR(100)  NOT NULL DEFAULT 'Colombia',
    departamento                  VARCHAR(100),
    municipio                     VARCHAR(100),
    direccion_principal           VARCHAR(250),
    complemento_direccion         VARCHAR(150),
    barrio                        VARCHAR(100),
    codigo_postal                 VARCHAR(20),
    email_contacto                VARCHAR(255),
    telefono_fijo                 VARCHAR(50),
    celular                       VARCHAR(50),
    sitio_web                     VARCHAR(255),
    direccion_correspondencia     VARCHAR(250),

    -- Responsable de la empresa
    responsable_nombre            VARCHAR(200),
    responsable_tipo_documento    VARCHAR(30),
    responsable_documento         VARCHAR(50),
    responsable_cargo             VARCHAR(100),
    responsable_rol               VARCHAR(50)   DEFAULT 'Representante Legal',
    responsable_email             VARCHAR(255),
    responsable_telefono          VARCHAR(50),
    responsable_observaciones     TEXT,

    -- Información tributaria colombiana
    regimen_tributario            VARCHAR(100)  DEFAULT 'Responsable de IVA',
    responsabilidad_iva           VARCHAR(50)   DEFAULT 'Responsable',
    codigo_ciiu                   VARCHAR(10),
    actividad_economica           VARCHAR(250),
    actividades_secundarias       VARCHAR(250),
    responsabilidades_tributarias TEXT,
    observaciones_tributarias     TEXT
);

CREATE INDEX IF NOT EXISTS idx_empresas_usuario ON empresas (usuario_id);

-- 3. TABLA: categorias_contables (Categorías DSS para análisis macro financiero)
CREATE TABLE IF NOT EXISTS categorias_contables (
    id                SERIAL        PRIMARY KEY,
    empresa_id        INT           NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    nombre            VARCHAR(150)  NOT NULL,
    tipo              VARCHAR(30)   NOT NULL
                      CHECK (tipo IN ('Ingreso', 'Costo', 'Gasto Fijo', 'Gasto Variable'))
);

CREATE INDEX IF NOT EXISTS idx_categorias_empresa ON categorias_contables (empresa_id);

-- 4. TABLA: cuentas_contables (Plan Único de Cuentas - PUC Colombia)
CREATE TABLE IF NOT EXISTS cuentas_contables (
    id                    SERIAL        PRIMARY KEY,
    empresa_id            INT           NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    codigo                VARCHAR(30)   NOT NULL,
    nombre                VARCHAR(200)  NOT NULL,
    descripcion           TEXT,
    clase                 INT           NOT NULL CHECK (clase BETWEEN 1 AND 9),
    grupo                 VARCHAR(10),
    nivel                 VARCHAR(30)   NOT NULL DEFAULT 'Subcuenta',
    naturaleza            VARCHAR(10)   NOT NULL CHECK (naturaleza IN ('Debito', 'Credito')),
    tipo_categoria        VARCHAR(50)   NOT NULL DEFAULT 'Gasto',
    categoria_id          INT           REFERENCES categorias_contables(id) ON DELETE SET NULL,
    maneja_terceros       BOOLEAN       NOT NULL DEFAULT FALSE,
    maneja_movimientos    BOOLEAN       NOT NULL DEFAULT TRUE,
    cuenta_padre_codigo   VARCHAR(30),
    estado                VARCHAR(20)   NOT NULL DEFAULT 'Activa'
                          CHECK (estado IN ('Activa', 'Inactiva')),
    CONSTRAINT uq_cuentas_empresa_codigo UNIQUE (empresa_id, codigo)
);

CREATE INDEX IF NOT EXISTS idx_cuentas_empresa ON cuentas_contables (empresa_id);
CREATE INDEX IF NOT EXISTS idx_cuentas_codigo  ON cuentas_contables (empresa_id, codigo);
CREATE INDEX IF NOT EXISTS idx_cuentas_clase   ON cuentas_contables (empresa_id, clase);

-- 5. TABLA: terceros (Clientes, Proveedores, Empleados, Contratistas)
CREATE TABLE IF NOT EXISTS terceros (
    id                    SERIAL        PRIMARY KEY,
    empresa_id            INT           NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    tipo_tercero          VARCHAR(50)   NOT NULL DEFAULT 'Proveedor',
    tipo_documento        VARCHAR(30)   NOT NULL DEFAULT 'NIT',
    numero_documento      VARCHAR(50)   NOT NULL,
    digito_verificacion   VARCHAR(2),
    razon_social          VARCHAR(250)  NOT NULL,
    nombre_comercial      VARCHAR(200),
    primer_nombre         VARCHAR(100),
    primer_apellido       VARCHAR(100),
    departamento          VARCHAR(100),
    municipio             VARCHAR(100),
    direccion             VARCHAR(250),
    email                 VARCHAR(255),
    telefono              VARCHAR(50),
    celular               VARCHAR(50),
    estado                VARCHAR(20)   NOT NULL DEFAULT 'Activo'
                          CHECK (estado IN ('Activo', 'Inactivo')),
    observaciones         TEXT,
    CONSTRAINT uq_terceros_empresa_doc UNIQUE (empresa_id, numero_documento)
);

CREATE INDEX IF NOT EXISTS idx_terceros_empresa ON terceros (empresa_id);
CREATE INDEX IF NOT EXISTS idx_terceros_doc     ON terceros (empresa_id, numero_documento);

-- 6. TABLA: centros_costo
CREATE TABLE IF NOT EXISTS centros_costo (
    id                    SERIAL        PRIMARY KEY,
    empresa_id            INT           NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    codigo                VARCHAR(30)   NOT NULL,
    nombre                VARCHAR(150)  NOT NULL,
    descripcion           TEXT,
    estado                VARCHAR(20)   NOT NULL DEFAULT 'Activo'
                          CHECK (estado IN ('Activo', 'Inactivo')),
    CONSTRAINT uq_centroscosto_empresa_codigo UNIQUE (empresa_id, codigo)
);

CREATE INDEX IF NOT EXISTS idx_centroscosto_empresa ON centros_costo (empresa_id);

-- 7. TABLA: transacciones_contables
CREATE TABLE IF NOT EXISTS transacciones_contables (
    id                        SERIAL        PRIMARY KEY,
    empresa_id                INT           NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    fecha                     DATE          NOT NULL,
    concepto                  VARCHAR(300)  NOT NULL,
    monto                     NUMERIC(18,2) NOT NULL,
    tipo                      VARCHAR(20)   NOT NULL CHECK (tipo IN ('Ingreso', 'Egreso')),
    categoria_id              INT           REFERENCES categorias_contables(id) ON DELETE SET NULL,
    observaciones             TEXT,

    -- Campos ampliados contables
    numero_transaccion        VARCHAR(50),
    cuenta_contable_id        INT           REFERENCES cuentas_contables(id) ON DELETE SET NULL,
    codigo_cuenta             VARCHAR(30),
    nombre_cuenta             VARCHAR(200),
    tercero_id                INT           REFERENCES terceros(id) ON DELETE SET NULL,
    tercero_tipo_doc          VARCHAR(30),
    tercero_numero_doc        VARCHAR(50),
    tercero_digito_verificacion VARCHAR(2),
    tercero_nit               VARCHAR(50),
    tercero_nombre            VARCHAR(200),
    centro_costo_id           INT           REFERENCES centros_costo(id) ON DELETE SET NULL,
    centro_costo_nombre       VARCHAR(100),
    naturaleza_movimiento     VARCHAR(10)   DEFAULT 'Debito'
                              CHECK (naturaleza_movimiento IN ('Debito', 'Credito')),
    monto_debito              NUMERIC(18,2) DEFAULT 0,
    monto_credito             NUMERIC(18,2) DEFAULT 0,
    tipo_documento_soporte    VARCHAR(50),
    numero_documento_soporte  VARCHAR(50),
    fecha_documento_soporte   DATE,
    descripcion               TEXT
);

CREATE INDEX IF NOT EXISTS idx_transacciones_empresa       ON transacciones_contables (empresa_id);
CREATE INDEX IF NOT EXISTS idx_transacciones_fecha         ON transacciones_contables (fecha);
CREATE INDEX IF NOT EXISTS idx_transacciones_tipo          ON transacciones_contables (tipo);
CREATE INDEX IF NOT EXISTS idx_transacciones_empresa_fecha ON transacciones_contables (empresa_id, fecha);
CREATE INDEX IF NOT EXISTS idx_tx_cuenta                  ON transacciones_contables (cuenta_contable_id);
CREATE INDEX IF NOT EXISTS idx_tx_tercero                 ON transacciones_contables (tercero_id);
CREATE INDEX IF NOT EXISTS idx_tx_cc                      ON transacciones_contables (centro_costo_id);

-- 8. TABLA: presupuestos
CREATE TABLE IF NOT EXISTS presupuestos (
    id                    SERIAL        PRIMARY KEY,
    empresa_id            INT           NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    periodo_mes           INT           NOT NULL CHECK (periodo_mes BETWEEN 1 AND 12),
    periodo_anio          INT           NOT NULL CHECK (periodo_anio >= 2020),
    categoria_id          INT           NOT NULL REFERENCES categorias_contables(id) ON DELETE CASCADE,
    monto_presupuestado   NUMERIC(18,2) NOT NULL CHECK (monto_presupuestado >= 0),

    -- Campos ampliados
    codigo                VARCHAR(50),
    nombre                VARCHAR(200),
    descripcion           TEXT,
    cuenta_contable_id    INT           REFERENCES cuentas_contables(id) ON DELETE SET NULL,
    tipo_presupuesto      VARCHAR(50)   DEFAULT 'Gasto Fijo',
    categoria_nombre      VARCHAR(100),
    estado                VARCHAR(30)   DEFAULT 'Activo',
    observaciones         TEXT
);

CREATE INDEX IF NOT EXISTS idx_presupuestos_empresa  ON presupuestos (empresa_id);
CREATE INDEX IF NOT EXISTS idx_presupuestos_periodo  ON presupuestos (empresa_id, periodo_anio, periodo_mes);
CREATE UNIQUE INDEX IF NOT EXISTS idx_presupuestos_unique
    ON presupuestos (empresa_id, periodo_anio, periodo_mes, categoria_id);

-- 9. TABLA: indicadores_dss
CREATE TABLE IF NOT EXISTS indicadores_dss (
    id                SERIAL        PRIMARY KEY,
    empresa_id        INT           NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    fecha_calculo     TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    flujo_caja        NUMERIC(18,2) NOT NULL DEFAULT 0,
    margen_operativo  NUMERIC(8,4)  NOT NULL DEFAULT 0,
    total_ingresos    NUMERIC(18,2) NOT NULL DEFAULT 0,
    total_gastos      NUMERIC(18,2) NOT NULL DEFAULT 0,
    nivel_alerta      VARCHAR(10)   NOT NULL DEFAULT 'Verde'
                      CHECK (nivel_alerta IN ('Verde', 'Amarillo', 'Rojo')),
    recomendacion     TEXT
);

CREATE INDEX IF NOT EXISTS idx_indicadores_empresa ON indicadores_dss (empresa_id);
CREATE INDEX IF NOT EXISTS idx_indicadores_fecha   ON indicadores_dss (fecha_calculo);

-- Usuario inicial de demostración
INSERT INTO usuarios (nombre, email, password_hash)
VALUES ('Contador Demo', 'demo@dss.com',
        '$2a$11$K3rZ7s8cQ8MhVqKZJzXOaO5F6.TjKQvR8JdYnXaKzVp3mN5qZfHnG')
ON CONFLICT (email) DO NOTHING;
