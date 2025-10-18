-- Seed test data for loan testing

-- Insert test Empresa
INSERT INTO empresa (empresanombre, empresartn, empresadireccion, empresatelefono, empresaemail, empresaestaactiva)
VALUES ('AlphaCredit Test', '0000000000000', 'Test Address 123', '00000000', 'test@alphatest.com', true)
ON CONFLICT DO NOTHING;

-- Insert test Sucursal (get empresa id first)
INSERT INTO sucursal (empresaid, sucursalnombre, sucursaldireccion, sucursaltelefono, sucursalestaactiva)
SELECT empresaid, 'Sucursal Central', 'Central Branch Address', '12345678', true
FROM empresa
WHERE empresanombre = 'AlphaCredit Test'
LIMIT 1
ON CONFLICT DO NOTHING;

SELECT 'Empresa and Sucursal created successfully' AS status;

-- Show IDs
SELECT empresaid, empresanombre FROM empresa WHERE empresanombre = 'AlphaCredit Test';
SELECT sucursalid, sucursalnombre FROM sucursal WHERE sucursalnombre = 'Sucursal Central';
