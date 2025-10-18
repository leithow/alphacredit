import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import prestamoService from '../../services/prestamoService';
import catalogService from '../../services/catalogService';
import fondoService from '../../services/fondoService';
import Button from '../../components/common/Button';
import ClienteSearchModal from '../../components/common/ClienteSearchModal';
import './PrestamoForm.css';

const PrestamoForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditMode = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [clienteSeleccionado, setClienteSeleccionado] = useState(null);
  const [showClienteModal, setShowClienteModal] = useState(false);
  const [amortizacionPreview, setAmortizacionPreview] = useState(null);
  const [catalogs, setCatalogs] = useState({
    estadosPrestamo: [],
    frecuenciasPago: [],
    destinosCredito: [],
    monedas: [],
    sucursales: [],
    fondos: [],
  });

  const [formData, setFormData] = useState({
    personaId: '',
    prestamoMonto: '',
    prestamoTasaInteres: '',
    prestamoPlazo: '',
    frecuenciaPagoId: '',
    destinoCreditoId: '',
    monedaId: '',
    sucursalId: '',
    fondoId: '',
    prestamoFechaDesembolso: new Date().toISOString().split('T')[0],
    prestamoObservaciones: '',
  });

  useEffect(() => {
    loadInitialData();
    if (isEditMode) {
      loadPrestamo();
    }
  }, [id]);

  const loadInitialData = async () => {
    try {
      setLoading(true);
      const [
        estadosData,
        frecuenciasData,
        destinosData,
        monedasData,
        sucursalesData,
        fondosData,
      ] = await Promise.all([
        catalogService.getEstadosPrestamo(),
        catalogService.getFrecuenciasPago(),
        catalogService.getDestinosCredito(),
        catalogService.getMonedas(),
        catalogService.getSucursales(),
        fondoService.getFondos(1, 1000, { soloActivos: true }),
      ]);

      setCatalogs({
        estadosPrestamo: estadosData,
        frecuenciasPago: frecuenciasData,
        destinosCredito: destinosData,
        monedas: monedasData,
        sucursales: sucursalesData,
        fondos: fondosData.data || [],
      });

      // Establecer valores por defecto
      if (monedasData.length > 0) {
        // Buscar peso dominicano (DOP) o tomar el primero
        const monedaDOP = monedasData.find((m) =>
          m.monedaCodigo?.toUpperCase().includes('DOP')
        );
        setFormData((prev) => ({
          ...prev,
          monedaId: monedaDOP ? monedaDOP.monedaId : monedasData[0].monedaId,
        }));
      }

      if (sucursalesData.length > 0) {
        setFormData((prev) => ({
          ...prev,
          sucursalId: sucursalesData[0].sucursalId,
        }));
      }
    } catch (error) {
      console.error('Error al cargar datos iniciales:', error);
      alert('Error al cargar los datos necesarios');
    } finally {
      setLoading(false);
    }
  };

  const loadPrestamo = async () => {
    try {
      setLoading(true);
      const prestamo = await prestamoService.getPrestamoById(id);
      setFormData({
        ...prestamo,
        prestamoFechaDesembolso: prestamo.prestamoFechaDesembolso
          ? prestamo.prestamoFechaDesembolso.split('T')[0]
          : '',
      });
    } catch (error) {
      console.error('Error al cargar préstamo:', error);
      alert('Error al cargar los datos del préstamo');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));

    // Limpiar preview si cambian valores críticos
    if (['prestamoMonto', 'prestamoTasaInteres', 'prestamoPlazo', 'frecuenciaPagoId'].includes(name)) {
      setAmortizacionPreview(null);
    }
  };

  const calcularAmortizacion = async () => {
    // Validar datos necesarios
    if (
      !formData.prestamoMonto ||
      !formData.prestamoTasaInteres ||
      !formData.prestamoPlazo ||
      !formData.frecuenciaPagoId
    ) {
      alert('Complete monto, tasa de interés, plazo y frecuencia de pago para calcular');
      return;
    }

    const monto = parseFloat(formData.prestamoMonto);
    const tasa = parseFloat(formData.prestamoTasaInteres);
    const plazo = parseInt(formData.prestamoPlazo);

    if (monto <= 0 || tasa <= 0 || plazo <= 0) {
      alert('Los valores deben ser mayores a cero');
      return;
    }

    try {
      setLoading(true);
      const frecuencia = catalogs.frecuenciasPago.find(
        (f) => f.frecuenciaPagoId === parseInt(formData.frecuenciaPagoId)
      );

      // Calcular amortización usando el servicio
      const amortizacion = await prestamoService.calcularAmortizacion({
        monto,
        tasaInteres: tasa,
        plazo,
        frecuenciaPagoDias: frecuencia.frecuenciaPagoDias,
      });

      setAmortizacionPreview(amortizacion);
    } catch (error) {
      console.error('Error al calcular amortización:', error);
      alert('Error al calcular la tabla de amortización');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    // Validaciones
    if (!formData.personaId) {
      alert('Seleccione un cliente');
      return;
    }

    if (!formData.prestamoMonto || parseFloat(formData.prestamoMonto) <= 0) {
      alert('El monto debe ser mayor a cero');
      return;
    }

    if (!formData.prestamoTasaInteres || parseFloat(formData.prestamoTasaInteres) <= 0) {
      alert('La tasa de interés debe ser mayor a cero');
      return;
    }

    if (!formData.prestamoPlazo || parseInt(formData.prestamoPlazo) <= 0) {
      alert('El plazo debe ser mayor a cero');
      return;
    }

    if (!formData.frecuenciaPagoId || !formData.monedaId || !formData.sucursalId) {
      alert('Complete todos los campos requeridos');
      return;
    }

    if (!formData.fondoId) {
      alert('Debe seleccionar un fondo para el desembolso');
      return;
    }

    const dataToSend = {
      personaId: parseInt(formData.personaId),
      prestamoMonto: parseFloat(formData.prestamoMonto),
      prestamoTasaInteres: parseFloat(formData.prestamoTasaInteres),
      prestamoPlazo: parseInt(formData.prestamoPlazo),
      frecuenciaPagoId: parseInt(formData.frecuenciaPagoId),
      destinoCreditoId: formData.destinoCreditoId ? parseInt(formData.destinoCreditoId) : null,
      monedaId: parseInt(formData.monedaId),
      sucursalId: parseInt(formData.sucursalId),
      fondoId: parseInt(formData.fondoId),
      prestamoFechaDesembolso: formData.prestamoFechaDesembolso + 'T00:00:00Z',
      prestamoObservaciones: formData.prestamoObservaciones || null,
    };

    try {
      setLoading(true);
      if (isEditMode) {
        await prestamoService.updatePrestamo(id, dataToSend);
        alert('Préstamo actualizado exitosamente');
      } else {
        await prestamoService.createPrestamo(dataToSend);
        alert('Préstamo creado exitosamente con su tabla de amortización');
      }
      navigate('/prestamos');
    } catch (error) {
      console.error('Error al guardar préstamo:', error);
      alert(
        error.response?.data?.message || error.response?.data?.title || 'Error al guardar el préstamo'
      );
    } finally {
      setLoading(false);
    }
  };

  const handleSelectCliente = (cliente) => {
    setClienteSeleccionado(cliente);
    setFormData((prev) => ({
      ...prev,
      personaId: cliente.personaId,
    }));
  };

  if (loading && isEditMode) {
    return <div className="form-loading">Cargando datos del préstamo...</div>;
  }

  return (
    <div className="prestamo-form-page">
      <div className="form-header">
        <h1>{isEditMode ? 'Editar Préstamo' : 'Nuevo Préstamo'}</h1>
        <p>Complete los datos del préstamo</p>
      </div>

      <form onSubmit={handleSubmit} className="prestamo-form">
        {/* DATOS DEL CLIENTE */}
        <div className="form-section">
          <h3>Cliente</h3>
          <div className="form-row">
            <div className="form-group full-width">
              <label>
                Cliente <span className="required">*</span>
              </label>
              {!clienteSeleccionado ? (
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setShowClienteModal(true)}
                  disabled={isEditMode}
                  style={{ width: '100%', justifyContent: 'center' }}
                >
                  🔍 Buscar y Seleccionar Cliente
                </Button>
              ) : (
                <div className="cliente-selected-card">
                  <div className="cliente-selected-info">
                    <div className="cliente-name">{clienteSeleccionado.personaNombreCompleto}</div>
                    <div className="cliente-details">
                      <span>
                        <strong>Identificación:</strong> {clienteSeleccionado.personaIdentificacion}
                      </span>
                      {clienteSeleccionado.personaEmail && (
                        <span>
                          <strong>Email:</strong> {clienteSeleccionado.personaEmail}
                        </span>
                      )}
                      {clienteSeleccionado.personaDireccion && (
                        <span>
                          <strong>Dirección:</strong> {clienteSeleccionado.personaDireccion}
                        </span>
                      )}
                    </div>
                  </div>
                  {!isEditMode && (
                    <Button
                      type="button"
                      variant="secondary"
                      size="small"
                      onClick={() => {
                        setClienteSeleccionado(null);
                        setFormData((prev) => ({ ...prev, personaId: '' }));
                      }}
                    >
                      Cambiar Cliente
                    </Button>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>

        {/* DATOS DEL PRÉSTAMO */}
        <div className="form-section">
          <h3>Datos del Préstamo</h3>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="prestamoMonto">
                Monto <span className="required">*</span>
              </label>
              <input
                type="number"
                id="prestamoMonto"
                name="prestamoMonto"
                value={formData.prestamoMonto}
                onChange={handleChange}
                step="0.01"
                min="0"
                placeholder="0.00"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="prestamoTasaInteres">
                Tasa de Interés (%) <span className="required">*</span>
              </label>
              <input
                type="number"
                id="prestamoTasaInteres"
                name="prestamoTasaInteres"
                value={formData.prestamoTasaInteres}
                onChange={handleChange}
                step="0.01"
                min="0"
                max="100"
                placeholder="0.00"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="prestamoPlazo">
                Plazo (cuotas) <span className="required">*</span>
              </label>
              <input
                type="number"
                id="prestamoPlazo"
                name="prestamoPlazo"
                value={formData.prestamoPlazo}
                onChange={handleChange}
                min="1"
                placeholder="12"
                required
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="frecuenciaPagoId">
                Frecuencia de Pago <span className="required">*</span>
              </label>
              <select
                id="frecuenciaPagoId"
                name="frecuenciaPagoId"
                value={formData.frecuenciaPagoId}
                onChange={handleChange}
                required
              >
                <option value="">Seleccione...</option>
                {catalogs.frecuenciasPago.map((freq) => (
                  <option key={freq.frecuenciaPagoId} value={freq.frecuenciaPagoId}>
                    {freq.frecuenciaPagoNombre} ({freq.frecuenciaPagoDias} días)
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="monedaId">
                Moneda <span className="required">*</span>
              </label>
              <select
                id="monedaId"
                name="monedaId"
                value={formData.monedaId}
                onChange={handleChange}
                required
              >
                <option value="">Seleccione...</option>
                {catalogs.monedas.map((moneda) => (
                  <option key={moneda.monedaId} value={moneda.monedaId}>
                    {moneda.monedaNombre} ({moneda.monedaCodigo})
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="prestamoFechaDesembolso">
                Fecha de Desembolso <span className="required">*</span>
              </label>
              <input
                type="date"
                id="prestamoFechaDesembolso"
                name="prestamoFechaDesembolso"
                value={formData.prestamoFechaDesembolso}
                onChange={handleChange}
                required
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="destinoCreditoId">Destino del Crédito</label>
              <select
                id="destinoCreditoId"
                name="destinoCreditoId"
                value={formData.destinoCreditoId}
                onChange={handleChange}
              >
                <option value="">Seleccione...</option>
                {catalogs.destinosCredito.map((destino) => (
                  <option key={destino.destinoCreditoId} value={destino.destinoCreditoId}>
                    {destino.destinoCreditoNombre}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="sucursalId">
                Sucursal <span className="required">*</span>
              </label>
              <select
                id="sucursalId"
                name="sucursalId"
                value={formData.sucursalId}
                onChange={handleChange}
                required
              >
                <option value="">Seleccione...</option>
                {catalogs.sucursales.map((sucursal) => (
                  <option key={sucursal.sucursalId} value={sucursal.sucursalId}>
                    {sucursal.sucursalNombre}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="fondoId">
                Fondo de Desembolso <span className="required">*</span>
              </label>
              <select
                id="fondoId"
                name="fondoId"
                value={formData.fondoId}
                onChange={handleChange}
                required
              >
                <option value="">Seleccione...</option>
                {catalogs.fondos
                  .filter((fondo) => !formData.monedaId || fondo.monedaId === parseInt(formData.monedaId))
                  .map((fondo) => (
                    <option
                      key={fondo.fondoId}
                      value={fondo.fondoId}
                      disabled={fondo.fondoSaldoActual < parseFloat(formData.prestamoMonto || 0)}
                    >
                      {fondo.fondoNombre} - Saldo: ${fondo.fondoSaldoActual.toFixed(2)}
                      {fondo.fondoSaldoActual < parseFloat(formData.prestamoMonto || 0) ? ' (Insuficiente)' : ''}
                    </option>
                  ))}
              </select>
              {formData.fondoId && catalogs.fondos.length > 0 && (
                <small style={{ color: '#666', marginTop: '4px', display: 'block' }}>
                  {(() => {
                    const fondoSeleccionado = catalogs.fondos.find(f => f.fondoId === parseInt(formData.fondoId));
                    if (fondoSeleccionado && formData.prestamoMonto) {
                      const saldoRestante = fondoSeleccionado.fondoSaldoActual - parseFloat(formData.prestamoMonto);
                      return `Saldo después del desembolso: $${saldoRestante.toFixed(2)}`;
                    }
                    return '';
                  })()}
                </small>
              )}
            </div>
          </div>

          <div className="form-row">
            <div className="form-group full-width">
              <label htmlFor="prestamoObservaciones">Observaciones</label>
              <textarea
                id="prestamoObservaciones"
                name="prestamoObservaciones"
                value={formData.prestamoObservaciones}
                onChange={handleChange}
                rows="3"
                placeholder="Observaciones adicionales sobre el préstamo..."
              />
            </div>
          </div>
        </div>

        {/* CALCULADORA DE AMORTIZACIÓN */}
        {!isEditMode && (
          <div className="form-section amortizacion-section">
            <div className="section-header">
              <h3>Vista Previa de Amortización</h3>
              <Button type="button" variant="outline" onClick={calcularAmortizacion} disabled={loading}>
                🧮 Calcular Amortización
              </Button>
            </div>

            {amortizacionPreview && amortizacionPreview.length > 0 && (
              <div className="amortizacion-preview">
                <div className="amortizacion-summary">
                  <div className="summary-item">
                    <span className="label">Total de Cuotas:</span>
                    <span className="value">{amortizacionPreview.length}</span>
                  </div>
                  <div className="summary-item">
                    <span className="label">Monto Total a Pagar:</span>
                    <span className="value">
                      L
                      {amortizacionPreview
                        .reduce((sum, cuota) => sum + cuota.cuota, 0)
                        .toFixed(2)}
                    </span>
                  </div>
                  <div className="summary-item">
                    <span className="label">Total Capital:</span>
                    <span className="value">
                      L
                      {amortizacionPreview
                        .reduce((sum, cuota) => sum + cuota.capital, 0)
                        .toFixed(2)}
                    </span>
                  </div>
                  <div className="summary-item">
                    <span className="label">Total Intereses:</span>
                    <span className="value">
                      L
                      {amortizacionPreview
                        .reduce((sum, cuota) => sum + cuota.interes, 0)
                        .toFixed(2)}
                    </span>
                  </div>
                </div>

                <div className="amortizacion-table-container">
                  <table className="amortizacion-table">
                    <thead>
                      <tr>
                        <th>#</th>
                        <th>Fecha Vencimiento</th>
                        <th>Capital</th>
                        <th>Interés</th>
                        <th>Cuota</th>
                        <th>Saldo Capital</th>
                      </tr>
                    </thead>
                    <tbody>
                      {amortizacionPreview.map((cuota, index) => (
                        <tr key={index}>
                          <td>{cuota.numeroCuota}</td>
                          <td>{new Date(cuota.fechaPago).toLocaleDateString()}</td>
                          <td className="amount">L{cuota.capital.toFixed(2)}</td>
                          <td className="amount">L{cuota.interes.toFixed(2)}</td>
                          <td className="amount"><strong>L{cuota.cuota.toFixed(2)}</strong></td>
                          <td className="amount">L{cuota.saldoCapital.toFixed(2)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
          </div>
        )}

        {/* ACCIONES */}
        <div className="form-actions">
          <Button type="button" variant="secondary" onClick={() => navigate('/prestamos')}>
            Cancelar
          </Button>
          <Button type="submit" variant="primary" disabled={loading}>
            {loading ? 'Guardando...' : isEditMode ? 'Actualizar' : 'Crear Préstamo'}
          </Button>
        </div>
      </form>

      {/* MODAL DE BÚSQUEDA DE CLIENTE */}
      <ClienteSearchModal
        isOpen={showClienteModal}
        onClose={() => setShowClienteModal(false)}
        onSelectCliente={handleSelectCliente}
      />
    </div>
  );
};

export default PrestamoForm;
