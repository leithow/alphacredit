-- Script para verificar configuración del sistema de abonos
-- Ejecutar con: psql -U postgres -d alphacredit -f verificar_abonos.sql

\echo '=== VERIFICACIÓN DEL SISTEMA DE ABONOS ==='
\echo ''

\echo '1. Verificando tabla fechasistema...'
SELECT
    fechasistemaid,
    fechasistemafecha,
    fechasistemaestaactiva,
    fechasistemausercrea
FROM fechasistema
WHERE fechasistemaestaactiva = true;

\echo ''
\echo '2. Verificando componentes de préstamo...'
SELECT
    componenteprestamoid,
    componenteprestamotipo,
    componenteprestam onombre
FROM componenteprestamo
ORDER BY componenteprestamoid;

\echo ''
\echo '3. Verificando estados de componente...'
SELECT
    estadocomponenteid,
    estadocomponentenombre
FROM estadocomponente
ORDER BY estadocomponenteid;

\echo ''
\echo '4. Verificando formas de pago con fondos...'
SELECT
    fp.formapagoid,
    fp.formapaganombre,
    fp.fondoid,
    f.fondonombre
FROM formapago fp
LEFT JOIN fondo f ON fp.fondoid = f.fondoid
ORDER BY fp.formapagoid;

\echo ''
\echo '5. Verificando préstamos con componentes...'
SELECT
    p.prestamoid,
    p.prestamonumero,
    COUNT(pc.prestamocomponenteid) as total_componentes,
    SUM(CASE WHEN cp.componenteprestamotipo = 'CAPITAL' THEN pc.prestamocomponentesaldo ELSE 0 END) as saldo_capital,
    SUM(CASE WHEN cp.componenteprestamotipo = 'INTERES' THEN pc.prestamocomponentesaldo ELSE 0 END) as saldo_interes,
    SUM(CASE WHEN cp.componenteprestamotipo = 'MORA' THEN pc.prestamocomponentesaldo ELSE 0 END) as saldo_mora
FROM prestamo p
LEFT JOIN prestamocomponente pc ON p.prestamoid = pc.prestamoid
LEFT JOIN componenteprestamo cp ON pc.componenteprestamoid = cp.componenteprestamoid
GROUP BY p.prestamoid, p.prestamonumero
LIMIT 5;

\echo ''
\echo '6. Verificando tabla pagodetalle...'
SELECT COUNT(*) as total_pagos FROM pagodetalle;
