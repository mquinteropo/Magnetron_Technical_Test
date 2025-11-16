-- Schema for billing_db (PostgreSQL 16)
-- Creates tables using snake_case and numeric(18,2) for money/quantity
-- Safe to run at init time via docker-entrypoint; uses IF NOT EXISTS where possible

SET client_min_messages TO WARNING;
SET timezone = 'UTC';
SET search_path = public;

-- PERSONA ---------------------------------------------------------------
CREATE TABLE IF NOT EXISTS persona (
  per_id        BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  per_nombre    VARCHAR(100) NOT NULL,
  per_apellido  VARCHAR(100) NOT NULL,
  per_tipodocumento VARCHAR(20) NOT NULL,
  per_documento VARCHAR(50) NOT NULL UNIQUE
);
CREATE INDEX IF NOT EXISTS idx_persona_documento ON persona(per_documento);

-- PRODUCTO --------------------------------------------------------------
CREATE TABLE IF NOT EXISTS producto (
  prod_id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  prod_descripcion VARCHAR(200) NOT NULL,
  prod_um          VARCHAR(10) NOT NULL,
  prod_precio      NUMERIC(18,2) NOT NULL CHECK (prod_precio >= 0),
  prod_costo       NUMERIC(18,2) NOT NULL CHECK (prod_costo >= 0)
);
CREATE INDEX IF NOT EXISTS idx_producto_descripcion ON producto(prod_descripcion);

-- FACTURA (ENCABEZADO) --------------------------------------------------
CREATE TABLE IF NOT EXISTS fact_encabezado (
  fenc_id     BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  fenc_numero VARCHAR(50) NOT NULL UNIQUE,
  fenc_fecha  TIMESTAMPTZ NOT NULL,
  zper_id     BIGINT NOT NULL REFERENCES persona(per_id)
);
CREATE INDEX IF NOT EXISTS idx_fenc_zper ON fact_encabezado(zper_id);
CREATE INDEX IF NOT EXISTS idx_fenc_fecha ON fact_encabezado(fenc_fecha);

-- FACTURA (DETALLE) -----------------------------------------------------
CREATE TABLE IF NOT EXISTS fact_detalle (
  fdet_id       BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  fdet_linea    INT NOT NULL CHECK (fdet_linea > 0),
  fdet_cantidad NUMERIC(18,2) NOT NULL CHECK (fdet_cantidad > 0),
  zprod_id      BIGINT NOT NULL REFERENCES producto(prod_id),
  zfenc_id      BIGINT NOT NULL REFERENCES fact_encabezado(fenc_id) ON DELETE CASCADE,
  unit_price    NUMERIC(18,2) NOT NULL CHECK (unit_price >= 0),
  line_total    NUMERIC(18,2) GENERATED ALWAYS AS (ROUND(fdet_cantidad * unit_price, 2)) STORED,
  CONSTRAINT uq_fdet_line UNIQUE (zfenc_id, fdet_linea)
);
CREATE INDEX IF NOT EXISTS idx_fdet_zprod ON fact_detalle(zprod_id);
CREATE INDEX IF NOT EXISTS idx_fdet_zfenc ON fact_detalle(zfenc_id);

-- END -------------------------------------------------------------------
