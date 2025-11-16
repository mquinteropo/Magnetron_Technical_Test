-- Seed data for quick validation
BEGIN;

-- Persons
INSERT INTO persona (per_nombre, per_apellido, per_tipodocumento, per_documento) VALUES
 ('Ana', 'Pérez', 'CC', '1001'),
 ('Bruno', 'García', 'CC', '1002'),
 ('Carla', 'López', 'CC', '1003')
ON CONFLICT (per_documento) DO NOTHING;

-- Products
INSERT INTO producto (prod_descripcion, prod_um, prod_precio, prod_costo) VALUES
 ('Teclado mecánico', 'UND', 250000.00, 180000.00),
 ('Mouse inalámbrico', 'UND', 80000.00, 50000.00),
 ('Monitor 27"', 'UND', 1200000.00, 950000.00)
ON CONFLICT DO NOTHING;

-- Example invoices
-- Invoice 1 for Ana
WITH p AS (
  SELECT per_id FROM persona WHERE per_documento = '1001'
),
ins AS (
  INSERT INTO fact_encabezado (fenc_numero, fenc_fecha, zper_id)
  SELECT 'F-0001', NOW(), per_id FROM p
  ON CONFLICT (fenc_numero) DO NOTHING
  RETURNING fenc_id
),
fe AS (
  SELECT COALESCE((SELECT fenc_id FROM ins), (SELECT fenc_id FROM fact_encabezado WHERE fenc_numero='F-0001')) AS fenc_id
)
INSERT INTO fact_detalle (fdet_linea, fdet_cantidad, zprod_id, zfenc_id, unit_price)
SELECT 1, 1.00, pr.prod_id, fe.fenc_id, pr.prod_precio
FROM fe
JOIN producto pr ON pr.prod_descripcion = 'Teclado mecánico';

-- Invoice 2 for Bruno with two lines
WITH p AS (
  SELECT per_id FROM persona WHERE per_documento = '1002'
),
ins AS (
  INSERT INTO fact_encabezado (fenc_numero, fenc_fecha, zper_id)
  SELECT 'F-0002', NOW(), per_id FROM p
  ON CONFLICT (fenc_numero) DO NOTHING
  RETURNING fenc_id
),
fe AS (
  SELECT COALESCE((SELECT fenc_id FROM ins), (SELECT fenc_id FROM fact_encabezado WHERE fenc_numero='F-0002')) AS fenc_id
)
INSERT INTO fact_detalle (fdet_linea, fdet_cantidad, zprod_id, zfenc_id, unit_price)
SELECT * FROM (
  SELECT 1 AS fdet_linea, 2.00::NUMERIC(18,2) AS fdet_cantidad, pr1.prod_id AS zprod_id, (SELECT fenc_id FROM fe) AS zfenc_id, pr1.prod_precio AS unit_price
  FROM producto pr1 WHERE pr1.prod_descripcion = 'Mouse inalámbrico'
  UNION ALL
  SELECT 2, 1.00, pr2.prod_id, (SELECT fenc_id FROM fe), pr2.prod_precio
  FROM producto pr2 WHERE pr2.prod_descripcion = 'Monitor 27"'
) s;

COMMIT;
