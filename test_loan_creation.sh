#!/bin/bash

API_URL="http://localhost:5000/api"

echo "=== Testing Loan Creation with Component Generation ==="
echo ""

# Step 1: Create test Empresa
echo "1. Creating test Empresa..."
EMPRESA_RESPONSE=$(curl -s -X POST "$API_URL/empresas" \
  -H "Content-Type: application/json" \
  -d '{
    "EmpresaNombre": "AlphaCredit Test",
    "EmpresaIdentificacion": "0000000000000",
    "EmpresaDireccion": "Test Address",
    "EmpresaTelefono": "00000000",
    "EmpresaEmail": "test@alphatest.com"
  }')

EMPRESA_ID=$(echo $EMPRESA_RESPONSE | python -m json.tool 2>/dev/null | grep -m 1 '"empresaId"' | sed 's/[^0-9]//g')
echo "Created Empresa ID: $EMPRESA_ID"
echo ""

# Step 2: Create test Sucursal
echo "2. Creating test Sucursal..."
SUCURSAL_RESPONSE=$(curl -s -X POST "$API_URL/sucursales" \
  -H "Content-Type: application/json" \
  -d "{
    \"EmpresaId\": $EMPRESA_ID,
    \"SucursalNombre\": \"Sucursal Test\",
    \"SucursalDireccion\": \"Test Branch Address\",
    \"SucursalTelefono\": \"00000000\"
  }")

SUCURSAL_ID=$(echo $SUCURSAL_RESPONSE | python -m json.tool 2>/dev/null | grep -m 1 '"sucursalId"' | sed 's/[^0-9]//g')
echo "Created Sucursal ID: $SUCURSAL_ID"
echo ""

# Step 3: Create test Persona
echo "3. Creating test Persona..."
PERSONA_RESPONSE=$(curl -s -X POST "$API_URL/personas" \
  -H "Content-Type: application/json" \
  -d '{
    "TipoIdentificacionId": 1,
    "PersonaIdentificacion": "0000000000000",
    "PersonaPrimerNombre": "Juan",
    "PersonaPrimerApellido": "Perez",
    "PersonaNombreCompleto": "Juan Perez",
    "PersonaEsNatural": true,
    "PersonaEsCliente": true,
    "SexoId": 1,
    "PersonaFechaNacimiento": "1990-01-01T00:00:00Z",
    "PersonaDireccion": "Test Address",
    "PersonaEmail": "juan.perez@test.com"
  }')

PERSONA_ID=$(echo $PERSONA_RESPONSE | python -m json.tool 2>/dev/null | grep -m 1 '"personaId"' | sed 's/[^0-9]//g')
echo "Created Persona ID: $PERSONA_ID"
echo ""

# Step 4: Test periodic loan creation (12 monthly payments)
echo "4. Creating test loan with PERIODIC payments (12 months)..."
LOAN_RESPONSE=$(curl -s -X POST "$API_URL/prestamos" \
  -H "Content-Type: application/json" \
  -d "{
    \"PersonaId\": $PERSONA_ID,
    \"SucursalId\": $SUCURSAL_ID,
    \"MonedaId\": 1,
    \"EstadoPrestamoId\": 1,
    \"FrecuenciaPagoId\": 4,
    \"PrestamoMonto\": 10000.00,
    \"PrestamoTasaInteres\": 15.0,
    \"PrestamoPlazo\": 12,
    \"EsAlVencimiento\": false,
    \"PrestamoObservaciones\": \"Test loan with periodic payments\"
  }")

LOAN_ID=$(echo $LOAN_RESPONSE | python -m json.tool 2>/dev/null | grep -m 1 '"prestamoId"' | sed 's/[^0-9]//g')
echo "Created Loan ID: $LOAN_ID"
echo ""
echo "Full Loan Response:"
echo $LOAN_RESPONSE | python -m json.tool
echo ""

# Step 5: Verify components were created
echo "5. Verifying components were generated..."
COMPONENTS=$(curl -s "$API_URL/prestamos/$LOAN_ID" | python -m json.tool)
echo "$COMPONENTS" | grep -A 5 "prestamoComponentes"
echo ""

# Step 6: Get amortization table
echo "6. Getting amortization table..."
curl -s "$API_URL/prestamos/$LOAN_ID/amortizacion" | python -m json.tool
echo ""

# Step 7: Test al vencimiento loan (single payment at end)
echo "7. Creating test loan AL VENCIMIENTO (single payment at end)..."
LOAN2_RESPONSE=$(curl -s -X POST "$API_URL/prestamos" \
  -H "Content-Type: application/json" \
  -d "{
    \"PersonaId\": $PERSONA_ID,
    \"SucursalId\": $SUCURSAL_ID,
    \"MonedaId\": 1,
    \"EstadoPrestamoId\": 1,
    \"FrecuenciaPagoId\": 4,
    \"PrestamoMonto\": 5000.00,
    \"PrestamoTasaInteres\": 12.0,
    \"PrestamoPlazo\": 6,
    \"EsAlVencimiento\": true,
    \"PrestamoObservaciones\": \"Test loan al vencimiento\"
  }")

LOAN2_ID=$(echo $LOAN2_RESPONSE | python -m json.tool 2>/dev/null | grep -m 1 '"prestamoId"' | sed 's/[^0-9]//g')
echo "Created Loan ID (al vencimiento): $LOAN2_ID"
echo ""
echo "Full Loan Response:"
echo $LOAN2_RESPONSE | python -m json.tool
echo ""

# Step 8: Get amortization for al vencimiento loan
echo "8. Getting amortization table for al vencimiento loan..."
curl -s "$API_URL/prestamos/$LOAN2_ID/amortizacion" | python -m json.tool
echo ""

echo "=== Test Complete ==="
