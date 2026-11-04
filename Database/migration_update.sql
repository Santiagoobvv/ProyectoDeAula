-- ═══════════════════════════════════════════════════════════════
-- MIGRACIÓN DE ACTUALIZACIÓN — JAS PANEL DE CONTROL FINANCIERO
-- Añade soporte completo para contabilidad colombiana PUC,
-- empresas ampliadas, terceros, centros de costo y transacciones avanzadas.
-- Idempotente (seguro de ejecutar múltiples veces sin perder datos)
-- ═══════════════════════════════════════════════════════════════

-- 1. AMPLIAR TABLA: empresas
ALTER TABLE empresas
    ADD COLUMN IF NOT EXISTS tipo_persona VARCHAR(20) DEFAULT 'Juridica',
    ADD COLUMN IF NOT EXISTS tipo_documento VARCHAR(30) DEFAULT 'NIT',
    ADD COLUMN IF NOT EXISTS numero_documento VARCHAR(50),
    ADD COLUMN IF NOT EXISTS digito_verificacion VARCHAR(2),
    ADD COLUMN IF NOT EXISTS razon_social VARCHAR(250),
    ADD COLUMN IF NOT EXISTS nombre_comercial VARCHAR(200),
    ADD COLUMN IF NOT EXISTS primer_nombre VARCHAR(100),
    ADD COLUMN IF NOT EXISTS segundo_nombre VARCHAR(100),
    ADD COLUMN IF NOT EXISTS primer_apellido VARCHAR(100),
    ADD COLUMN IF NOT EXISTS segundo_apellido VARCHAR(100),
    ADD COLUMN IF NOT EXISTS estado VARCHAR(20) DEFAULT 'Activa',
    ADD COLUMN IF NOT EXISTS pais VARCHAR(100) DEFAULT 'Colombia',
    ADD COLUMN IF NOT EXISTS departamento VARCHAR(100),
    ADD COLUMN IF NOT EXISTS municipio VARCHAR(100),
    ADD COLUMN IF NOT EXISTS direccion_principal VARCHAR(250),
    ADD COLUMN IF NOT EXISTS complemento_direccion VARCHAR(150),
    ADD COLUMN IF NOT EXISTS barrio VARCHAR(100),
    ADD COLUMN IF NOT EXISTS codigo_postal VARCHAR(20),
    ADD COLUMN IF NOT EXISTS email_contacto VARCHAR(255),
    ADD COLUMN IF NOT EXISTS telefono_fijo VARCHAR(50),
    ADD COLUMN IF NOT EXISTS celular VARCHAR(50),
    ADD COLUMN IF NOT EXISTS sitio_web VARCHAR(255),
    ADD COLUMN IF NOT EXISTS direccion_correspondencia VARCHAR(250),
    ADD COLUMN IF NOT EXISTS responsable_nombre VARCHAR(200),
    ADD COLUMN IF NOT EXISTS responsable_tipo_documento VARCHAR(30),
    ADD COLUMN IF NOT EXISTS responsable_documento VARCHAR(50),
    ADD COLUMN IF NOT EXISTS responsable_cargo VARCHAR(100),
    ADD COLUMN IF NOT EXISTS responsable_rol VARCHAR(50) DEFAULT 'Representante Legal',
    ADD COLUMN IF NOT EXISTS responsable_email VARCHAR(255),
    ADD COLUMN IF NOT EXISTS responsable_telefono VARCHAR(50),
    ADD COLUMN IF NOT EXISTS responsable_observaciones TEXT,
    ADD COLUMN IF NOT EXISTS regimen_tributario VARCHAR(100) DEFAULT 'Responsable de IVA',
    ADD COLUMN IF NOT EXISTS responsabilidad_iva VARCHAR(50) DEFAULT 'Responsable',
    ADD COLUMN IF NOT EXISTS codigo_ciiu VARCHAR(10),
    ADD COLUMN IF NOT EXISTS actividad_economica VARCHAR(250),
    ADD COLUMN IF NOT EXISTS actividades_secundarias VARCHAR(250),
    ADD COLUMN IF NOT EXISTS responsabilidades_tributarias TEXT,
    ADD COLUMN IF NOT EXISTS observaciones_tributarias TEXT;

-- Sincronizar datos existentes si es necesario
UPDATE empresas 
SET razon_social = nombre_empresa 
WHERE razon_social IS NULL AND nombre_empresa IS NOT NULL;

UPDATE empresas 
SET numero_documento = nit 
WHERE numero_documento IS NULL AND nit IS NOT NULL;

-- 2. NUEVA TABLA: cuentas_contables (Plan Único de Cuentas - PUC Colombia)
CREATE TABLE IF NOT EXISTS cuentas_contables (
    id                    SERIAL PRIMARY KEY,
    empresa_id            INT NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    codigo                VARCHAR(30) NOT NULL,
    nombre                VARCHAR(200) NOT NULL,
    descripcion           TEXT,
    clase                 INT NOT NULL CHECK (clase BETWEEN 1 AND 9),
    grupo                 VARCHAR(10),
    nivel                 VARCHAR(30) NOT NULL DEFAULT 'Subcuenta',
    naturaleza            VARCHAR(10) NOT NULL CHECK (naturaleza IN ('Debito', 'Credito')),
    tipo_categoria        VARCHAR(50) NOT NULL DEFAULT 'Gasto',
    categoria_id          INT REFERENCES categorias_contables(id) ON DELETE SET NULL,
    maneja_terceros       BOOLEAN NOT NULL DEFAULT FALSE,
    maneja_movimientos    BOOLEAN NOT NULL DEFAULT TRUE,
    cuenta_padre_codigo   VARCHAR(30),
    estado                VARCHAR(20) NOT NULL DEFAULT 'Activa' CHECK (estado IN ('Activa', 'Inactiva')),
    CONSTRAINT uq_cuentas_empresa_codigo UNIQUE (empresa_id, codigo)
);

CREATE INDEX IF NOT EXISTS idx_cuentas_empresa ON cuentas_contables (empresa_id);
CREATE INDEX IF NOT EXISTS idx_cuentas_codigo  ON cuentas_contables (empresa_id, codigo);
CREATE INDEX IF NOT EXISTS idx_cuentas_clase   ON cuentas_contables (empresa_id, clase);

-- 3. NUEVA TABLA: terceros (Clientes, Proveedores, Empleados, Contratistas)
CREATE TABLE IF NOT EXISTS terceros (
    id                    SERIAL PRIMARY KEY,
    empresa_id            INT NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    tipo_tercero          VARCHAR(50) NOT NULL DEFAULT 'Proveedor',
    tipo_documento        VARCHAR(30) NOT NULL DEFAULT 'NIT',
    numero_documento      VARCHAR(50) NOT NULL,
    digito_verificacion   VARCHAR(2),
    razon_social          VARCHAR(250) NOT NULL,
    nombre_comercial      VARCHAR(200),
    primer_nombre         VARCHAR(100),
    primer_apellido       VARCHAR(100),
    departamento          VARCHAR(100),
    municipio             VARCHAR(100),
    direccion             VARCHAR(250),
    email                 VARCHAR(255),
    telefono              VARCHAR(50),
    celular               VARCHAR(50),
    estado                VARCHAR(20) NOT NULL DEFAULT 'Activo' CHECK (estado IN ('Activo', 'Inactivo')),
    observaciones         TEXT,
    CONSTRAINT uq_terceros_empresa_doc UNIQUE (empresa_id, numero_documento)
);

CREATE INDEX IF NOT EXISTS idx_terceros_empresa ON terceros (empresa_id);
CREATE INDEX IF NOT EXISTS idx_terceros_doc     ON terceros (empresa_id, numero_documento);

-- 4. NUEVA TABLA: centros_costo
CREATE TABLE IF NOT EXISTS centros_costo (
    id                    SERIAL PRIMARY KEY,
    empresa_id            INT NOT NULL REFERENCES empresas(id) ON DELETE CASCADE,
    codigo                VARCHAR(30) NOT NULL,
    nombre                VARCHAR(150) NOT NULL,
    descripcion           TEXT,
    estado                VARCHAR(20) NOT NULL DEFAULT 'Activo' CHECK (estado IN ('Activo', 'Inactivo')),
    CONSTRAINT uq_centroscosto_empresa_codigo UNIQUE (empresa_id, codigo)
);

CREATE INDEX IF NOT EXISTS idx_centroscosto_empresa ON centros_costo (empresa_id);

-- 5. AMPLIAR TABLA: presupuestos
ALTER TABLE presupuestos
    ADD COLUMN IF NOT EXISTS codigo VARCHAR(50),
    ADD COLUMN IF NOT EXISTS nombre VARCHAR(200),
    ADD COLUMN IF NOT EXISTS descripcion TEXT,
    ADD COLUMN IF NOT EXISTS cuenta_contable_id INT REFERENCES cuentas_contables(id) ON DELETE SET NULL,
    ADD COLUMN IF NOT EXISTS tipo_presupuesto VARCHAR(50) DEFAULT 'Gasto Fijo',
    ADD COLUMN IF NOT EXISTS categoria_nombre VARCHAR(100),
    ADD COLUMN IF NOT EXISTS estado VARCHAR(30) DEFAULT 'Activo',
    ADD COLUMN IF NOT EXISTS observaciones TEXT;

-- 6. AMPLIAR TABLA: transacciones_contables
ALTER TABLE transacciones_contables
    ADD COLUMN IF NOT EXISTS numero_transaccion VARCHAR(50),
    ADD COLUMN IF NOT EXISTS cuenta_contable_id INT REFERENCES cuentas_contables(id) ON DELETE SET NULL,
    ADD COLUMN IF NOT EXISTS codigo_cuenta VARCHAR(30),
    ADD COLUMN IF NOT EXISTS nombre_cuenta VARCHAR(200),
    ADD COLUMN IF NOT EXISTS tercero_id INT REFERENCES terceros(id) ON DELETE SET NULL,
    ADD COLUMN IF NOT EXISTS tercero_tipo_doc VARCHAR(30),
    ADD COLUMN IF NOT EXISTS tercero_numero_doc VARCHAR(50),
    ADD COLUMN IF NOT EXISTS tercero_digito_verificacion VARCHAR(2),
    ADD COLUMN IF NOT EXISTS tercero_nit VARCHAR(50),
    ADD COLUMN IF NOT EXISTS tercero_nombre VARCHAR(200),
    ADD COLUMN IF NOT EXISTS centro_costo_id INT REFERENCES centros_costo(id) ON DELETE SET NULL,
    ADD COLUMN IF NOT EXISTS centro_costo_nombre VARCHAR(100),
    ADD COLUMN IF NOT EXISTS naturaleza_movimiento VARCHAR(10) DEFAULT 'Debito',
    ADD COLUMN IF NOT EXISTS monto_debito NUMERIC(18,2) DEFAULT 0,
    ADD COLUMN IF NOT EXISTS monto_credito NUMERIC(18,2) DEFAULT 0,
    ADD COLUMN IF NOT EXISTS tipo_documento_soporte VARCHAR(50),
    ADD COLUMN IF NOT EXISTS numero_documento_soporte VARCHAR(50),
    ADD COLUMN IF NOT EXISTS fecha_documento_soporte DATE,
    ADD COLUMN IF NOT EXISTS descripcion TEXT;

-- Sincronizar datos históricos de transacciones contables desde terceros
UPDATE transacciones_contables t
SET 
    tercero_tipo_doc = terc.tipo_documento,
    tercero_numero_doc = terc.numero_documento,
    tercero_digito_verificacion = terc.digito_verificacion
FROM terceros terc
WHERE t.tercero_id = terc.id AND t.tercero_tipo_doc IS NULL;

-- Fallback para transacciones con tercero_nit pero sin tercero_id
UPDATE transacciones_contables
SET 
    tercero_tipo_doc = COALESCE(tercero_tipo_doc, 'NIT'),
    tercero_numero_doc = COALESCE(tercero_numero_doc, tercero_nit)
WHERE tercero_nit IS NOT NULL AND tercero_numero_doc IS NULL;

CREATE INDEX IF NOT EXISTS idx_tx_cuenta  ON transacciones_contables (cuenta_contable_id);
CREATE INDEX IF NOT EXISTS idx_tx_tercero ON transacciones_contables (tercero_id);
CREATE INDEX IF NOT EXISTS idx_tx_cc      ON transacciones_contables (centro_costo_id);
