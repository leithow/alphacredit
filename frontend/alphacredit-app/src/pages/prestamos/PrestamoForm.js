import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import prestamoService from '../../services/prestamoService';
import catalogService from '../../services/catalogService';
import fondoService from '../../services/fondoService';
import garantiaService from '../../services/garantiaService';
import Button from '../../components/common/Button';
import ClienteSearchModal from '../../components/common/ClienteSearchModal';
import PrestamoDocumentosModal from '../../components/common/PrestamoDocumentosModal';
import { formatCurrency } from '../../utils/currency';
import './PrestamoForm.css';

const PrestamoForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditMode = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [clienteSeleccionado, setClienteSeleccionado] = useState(null);
  const [showClienteModal, setShowClienteModal] = useState(false);
  const [showDocumentosModal, setShowDocumentosModal] = useState(false);
  const [prestamoData, setPrestamoData] = useState(null);
  const [amortizacionPreview, setAmortizacionPreview] = useState(null);
  const [garantiasDisponibles, setGarantiasDisponibles] = useState([]);
  const [garantiasSeleccionadas, setGarantiasSeleccionadas] = useState([]);
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
      setPrestamoData(prestamo); // Guardar datos completos para el modal
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
      garantias: garantiasSeleccionadas
        .filter(g => g.montoComprometido > 0)
        .map(g => ({
          garantiaId: g.garantiaId,
          montoComprometido: g.montoComprometido
        }))
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

  const handleSelectCliente = async (cliente) => {
    setClienteSeleccionado(cliente);
    setFormData((prev) => ({
      ...prev,
      personaId: cliente.personaId,
    }));

    // Cargar garantías disponibles del cliente
    await loadGarantiasDisponibles(cliente.personaId);
  };

  const loadGarantiasDisponibles = async (personaId) => {
    try {
      const garantias = await garantiaService.getGarantiasDisponibles(personaId);
      setGarantiasDisponibles(garantias);
    } catch (error) {
      console.error('Error al cargar garantías disponibles:', error);
    }
  };

  const agregarGarantia = (garantia) => {
    // Verificar que no esté ya agregada
    if (garantiasSeleccionadas.find(g => g.garantiaId === garantia.garantiaId)) {
      alert('Esta garantía ya fue agregada');
      return;
    }

    // Agregar con monto comprometido inicial
    setGarantiasSeleccionadas([...garantiasSeleccionadas, {
      garantiaId: garantia.garantiaId,
      garantiaDescripcion: garantia.garantiaDescripcion,
      tipoGarantiaNombre: garantia.tipoGarantiaNombre,
      valorDisponible: garantia.valorDisponible,
      montoComprometido: 0
    }]);
  };

  const quitarGarantia = (garantiaId) => {
    setGarantiasSeleccionadas(garantiasSeleccionadas.filter(g => g.garantiaId !== garantiaId));
  };

  const actualizarMontoGarantia = (garantiaId, monto) => {
    setGarantiasSeleccionadas(garantiasSeleccionadas.map(g =>
      g.garantiaId === garantiaId
        ? { ...g, montoComprometido: parseFloat(monto) || 0 }
        : g
    ));
  };

  if (loading && isEditMode) {
    return <div className="form-loading">Cargando datos del préstamo...</div>;
  }

  // Modo vista (solo lectura) si es edición
  const isViewMode = isEditMode;

  return (
    <div className="prestamo-form-page">
      <div className="form-header">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '10px' }}>
          <div>
            <h1 style={{ margin: 0 }}>{isViewMode ? '📋 Consultar Préstamo' : 'Nuevo Préstamo'}</h1>
            <p style={{ margin: '5px 0 0 0' }}>{isViewMode ? 'Información del préstamo (solo lectura)' : 'Complete los datos del préstamo'}</p>
          </div>
          {isViewMode && prestamoData && (
            <Button
              variant="primary"
              onClick={() => setShowDocumentosModal(true)}
              style={{ display: 'flex', alignItems: 'center', gap: '8px' }}
            >
              📄 Ver Documentos
            </Button>
          )}
        </div>
        {isViewMode && (
          <div style={{ background: '#fff3cd', padding: '10px 15px', borderRadius: '5px', marginTop: '10px' }}>
            <strong>ℹ️ Modo Solo Lectura:</strong> Los préstamos no pueden editarse por razones de integridad financiera y auditoría.
          </div>
        )}
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
                  disabled={isViewMode}
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
                  {!isViewMode && (
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
                disabled={isViewMode}
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
                disabled={isViewMode}
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
                disabled={isViewMode}
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
                disabled={isViewMode}
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
                disabled={isViewMode}
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
                disabled={isViewMode}
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
                disabled={isViewMode}
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
                disabled={isViewMode}
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
                disabled={isViewMode}
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
                      {fondo.fondoNombre} - Saldo: {formatCurrency(fondo.fondoSaldoActual)}
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
                      return `Saldo después del desembolso: ${formatCurrency(saldoRestante)}`;
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
                disabled={isViewMode}
              />
            </div>
          </div>
        </div>

        {/* GARANTÍAS */}
        {!isViewMode && clienteSeleccionado && (
          <div className="form-section garantias-section">
            <h3>Garantías (Opcional)</h3>
            <p className="section-description">
              Agregue garantías para respaldar este préstamo. Las garantías prendarias e hipotecarias tienen un valor disponible limitado.
            </p>

            {/* Garantías Disponibles */}
            {garantiasDisponibles.length > 0 && (
              <div className="garantias-disponibles">
                <h4>Garantías Disponibles del Cliente</h4>
                <div className="garantias-grid">
                  {garantiasDisponibles
                    .filter(g => !garantiasSeleccionadas.find(gs => gs.garantiaId === g.garantiaId))
                    .map((garantia) => (
                      <div key={garantia.garantiaId} className="garantia-card disponible">
                        <div className="garantia-header">
                          <span className={`tipo-badge tipo-${garantia.tipoGarantiaNombre?.toLowerCase()}`}>
                            {garantia.tipoGarantiaNombre}
                          </span>
                          <Button
                            type="button"
                            variant="primary"
                            size="small"
                            onClick={() => agregarGarantia(garantia)}
                          >
                            + Agregar
                          </Button>
                        </div>
                        <div className="garantia-body">
                          <p className="garantia-descripcion">{garantia.garantiaDescripcion}</p>
                          <div className="garantia-valores">
                            <div className="valor-item">
                              <span className="label">Valor Realizable:</span>
                              <span className="value">{formatCurrency(garantia.garantiaValorRealizable || 0)}</span>
                            </div>
                            {!garantia.tipoGarantiaNombre?.toUpperCase().includes('FIDUCIARIA') && (
                              <>
                                <div className="valor-item">
                                  <span className="label">Comprometido:</span>
                                  <span className="value comprometido">{formatCurrency(garantia.montoComprometido || 0)}</span>
                                </div>
                                <div className="valor-item">
                                  <span className="label">Disponible:</span>
                                  <span className="value disponible">{formatCurrency(garantia.valorDisponible || 0)}</span>
                                </div>
                              </>
                            )}
                          </div>
                        </div>
                      </div>
                    ))}
                </div>
              </div>
            )}

            {/* Garantías Seleccionadas */}
            {garantiasSeleccionadas.length > 0 && (
              <div className="garantias-seleccionadas">
                <h4>Garantías Agregadas</h4>
                <div className="garantias-list">
                  {garantiasSeleccionadas.map((garantia) => (
                    <div key={garantia.garantiaId} className="garantia-card seleccionada">
                      <div className="garantia-header">
                        <span className={`tipo-badge tipo-${garantia.tipoGarantiaNombre?.toLowerCase()}`}>
                          {garantia.tipoGarantiaNombre}
                        </span>
                        <Button
                          type="button"
                          variant="danger"
                          size="small"
                          onClick={() => quitarGarantia(garantia.garantiaId)}
                        >
                          Quitar
                        </Button>
                      </div>
                      <div className="garantia-body">
                        <p className="garantia-descripcion">{garantia.garantiaDescripcion}</p>
                        <div className="monto-comprometido-input">
                          <label htmlFor={`monto-${garantia.garantiaId}`}>
                            Monto a Comprometer: <span className="required">*</span>
                          </label>
                          <input
                            type="number"
                            id={`monto-${garantia.garantiaId}`}
                            value={garantia.montoComprometido || ''}
                            onChange={(e) => actualizarMontoGarantia(garantia.garantiaId, e.target.value)}
                            step="0.01"
                            min="0"
                            max={garantia.valorDisponible || undefined}
                            placeholder="0.00"
                            required
                          />
                          {!garantia.tipoGarantiaNombre?.toUpperCase().includes('FIDUCIARIA') && (
                            <small>Disponible: {formatCurrency(garantia.valorDisponible || 0)}</small>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
                <div className="garantias-summary">
                  <strong>Total Comprometido:</strong>
                  <span className="total-comprometido">
                    {formatCurrency(garantiasSeleccionadas.reduce((sum, g) => sum + (g.montoComprometido || 0), 0))}
                  </span>
                </div>
              </div>
            )}

            {garantiasDisponibles.length === 0 && (
              <div className="no-garantias">
                <p>Este cliente no tiene garantías disponibles.</p>
                <p>
                  <small>Puede crear el préstamo sin garantías o agregar garantías desde el módulo de Garantías.</small>
                </p>
              </div>
            )}
          </div>
        )}

        {/* CALCULADORA DE AMORTIZACIÓN */}
        {!isViewMode && (
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
                      {formatCurrency(amortizacionPreview.reduce((sum, cuota) => sum + cuota.cuota, 0))}
                    </span>
                  </div>
                  <div className="summary-item">
                    <span className="label">Total Capital:</span>
                    <span className="value">
                      {formatCurrency(amortizacionPreview.reduce((sum, cuota) => sum + cuota.capital, 0))}
                    </span>
                  </div>
                  <div className="summary-item">
                    <span className="label">Total Intereses:</span>
                    <span className="value">
                      {formatCurrency(amortizacionPreview.reduce((sum, cuota) => sum + cuota.interes, 0))}
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
                          <td className="amount">{formatCurrency(cuota.capital)}</td>
                          <td className="amount">{formatCurrency(cuota.interes)}</td>
                          <td className="amount"><strong>{formatCurrency(cuota.cuota)}</strong></td>
                          <td className="amount">{formatCurrency(cuota.saldoCapital)}</td>
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
            {isViewMode ? '← Volver' : 'Cancelar'}
          </Button>
          {!isViewMode && (
            <Button type="submit" variant="primary" disabled={loading}>
              {loading ? 'Guardando...' : 'Crear Préstamo'}
            </Button>
          )}
        </div>
      </form>

      {/* MODAL DE BÚSQUEDA DE CLIENTE */}
      <ClienteSearchModal
        isOpen={showClienteModal}
        onClose={() => setShowClienteModal(false)}
        onSelectCliente={handleSelectCliente}
      />

      {/* MODAL DE DOCUMENTOS DEL PRÉSTAMO */}
      {isViewMode && prestamoData && (
        <PrestamoDocumentosModal
          isOpen={showDocumentosModal}
          onClose={() => setShowDocumentosModal(false)}
          prestamo={prestamoData}
        />
      )}
    </div>
  );
};

export default PrestamoForm;
