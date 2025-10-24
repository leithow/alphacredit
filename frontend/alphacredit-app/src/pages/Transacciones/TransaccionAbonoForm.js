import React, { useState, useEffect } from 'react';
import transaccionService from '../../services/transaccionService';
import EstadoCuentaModal from './EstadoCuentaModal';
import './TransaccionAbonoForm.css';

const TransaccionAbonoForm = ({ prestamo, onCancelar, onCompletado }) => {
  // Estado del formulario
  const [tipoAbono, setTipoAbono] = useState('PARCIAL');
  const [monto, setMonto] = useState('');
  const [formaPagoId, setFormaPagoId] = useState('');
  const [documentoReferencia, setDocumentoReferencia] = useState('');
  const [numeroCuota, setNumeroCuota] = useState('');
  const [observaciones, setObservaciones] = useState('');
  const [distribucion, setDistribucion] = useState({
    mora: '',
    interes: '',
    capital: ''
  });

  // Estado de datos
  const [formasPago, setFormasPago] = useState([]);
  const [estadoCuenta, setEstadoCuenta] = useState(null);
  const [cargandoEstadoCuenta, setCargandoEstadoCuenta] = useState(false);
  const [procesando, setProcesando] = useState(false);
  const [error, setError] = useState(null);
  const [mostrarEstadoCuenta, setMostrarEstadoCuenta] = useState(false);

  // Cargar formas de pago al montar el componente
  useEffect(() => {
    cargarFormasPago();
    cargarEstadoCuenta();
  }, [prestamo.prestamoId]);

  const cargarFormasPago = async () => {
    try {
      const formas = await transaccionService.obtenerFormasPago();
      setFormasPago(formas.filter(f => f.formaPagoEstaActiva));
    } catch (error) {
      console.error('Error al cargar formas de pago:', error);
      setError('No se pudieron cargar las formas de pago');
    }
  };

  const cargarEstadoCuenta = async () => {
    setCargandoEstadoCuenta(true);
    try {
      const estado = await transaccionService.obtenerEstadoCuenta(prestamo.prestamoId);
      setEstadoCuenta(estado);
    } catch (error) {
      console.error('Error al cargar estado de cuenta:', error);
      setError('No se pudo cargar el estado de cuenta');
    } finally {
      setCargandoEstadoCuenta(false);
    }
  };

  // Calcular distribución automática cuando cambia el monto
  useEffect(() => {
    if (tipoAbono === 'PARCIAL' && monto && estadoCuenta) {
      calcularDistribucionAutomatica();
    }
  }, [monto, tipoAbono, estadoCuenta]);

  const calcularDistribucionAutomatica = () => {
    const montoNumerico = parseFloat(monto) || 0;
    let restante = montoNumerico;

    // Prioridad: MORA → INTERES → CAPITAL
    const mora = Math.min(restante, estadoCuenta.saldoMora || 0);
    restante -= mora;

    const interes = Math.min(restante, estadoCuenta.saldoInteres || 0);
    restante -= interes;

    const capital = Math.min(restante, estadoCuenta.saldoCapital || 0);

    setDistribucion({
      mora: mora.toFixed(2),
      interes: interes.toFixed(2),
      capital: capital.toFixed(2)
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);

    // Validaciones
    if (!validarFormulario()) {
      return;
    }

    setProcesando(true);

    try {
      const abonoData = construirDatosAbono();
      console.log('Datos del abono a enviar:', abonoData);

      const respuesta = await transaccionService.aplicarAbonoPrestamo(
        prestamo.prestamoId,
        abonoData
      );

      console.log('Respuesta del servidor:', respuesta);

      // Descargar recibo automáticamente
      descargarRecibo(respuesta.movimientoPrestamoId);

      alert(
        `Abono aplicado exitosamente!\n\n` +
        `Movimiento ID: ${respuesta.movimientoPrestamoId}\n` +
        `Capital pagado: L${(respuesta.montoCapital || 0).toFixed(2)}\n` +
        `Interés pagado: L${(respuesta.montoInteres || 0).toFixed(2)}\n` +
        `Mora pagada: L${(respuesta.montoMora || 0).toFixed(2)}\n` +
        `Total: L${(respuesta.montoAplicado || 0).toFixed(2)}\n\n` +
        `${respuesta.mensaje || ''}\n\n` +
        `El recibo se ha descargado automáticamente.`
      );

      onCompletado();
    } catch (error) {
      console.error('Error al aplicar abono:', error);
      setError(error.mensaje || 'Error al procesar el abono. Por favor intente nuevamente.');
    } finally {
      setProcesando(false);
    }
  };

  const validarFormulario = () => {
    if (!monto || parseFloat(monto) <= 0) {
      setError('El monto debe ser mayor a cero');
      return false;
    }

    if (!formaPagoId) {
      setError('Debe seleccionar una forma de pago');
      return false;
    }

    const formaPagoSeleccionada = formasPago.find(f => f.formaPagoId === parseInt(formaPagoId));
    if (formaPagoSeleccionada?.formaPagoNombre?.toLowerCase().includes('transferencia') &&
        !documentoReferencia.trim()) {
      setError('Debe ingresar el número de referencia de la transferencia');
      return false;
    }

    if (tipoAbono === 'CUOTA' && !numeroCuota) {
      setError('Debe especificar el número de cuota');
      return false;
    }

    if (tipoAbono === 'PARCIAL') {
      const totalDistribucion = parseFloat(distribucion.mora || 0) +
                                parseFloat(distribucion.interes || 0) +
                                parseFloat(distribucion.capital || 0);

      if (Math.abs(totalDistribucion - parseFloat(monto)) > 0.01) {
        setError('La distribución debe sumar el monto total del abono');
        return false;
      }
    }

    return true;
  };

  const descargarRecibo = async (movimientoPrestamoId) => {
    try {
      const url = `http://localhost:5000/api/prestamos/abonos/${movimientoPrestamoId}/recibo`;

      // Abrir en nueva ventana para descargar
      window.open(url, '_blank');
    } catch (error) {
      console.error('Error al descargar recibo:', error);
      // No mostrar error al usuario, solo log
    }
  };

  const construirDatosAbono = () => {
    const abonoData = {
      monto: parseFloat(monto),
      formaPagoId: parseInt(formaPagoId),
      tipoAbono: tipoAbono,
      observaciones: observaciones.trim()
    };

    // Agregar documento de referencia si existe
    if (documentoReferencia.trim()) {
      abonoData.documentoReferencia = documentoReferencia.trim();
    }

    // Agregar número de cuota si el tipo es CUOTA
    if (tipoAbono === 'CUOTA' && numeroCuota) {
      abonoData.numeroCuota = parseInt(numeroCuota);
    }

    // Agregar distribución manual si el tipo es PARCIAL y hay distribución personalizada
    if (tipoAbono === 'PARCIAL') {
      const totalDistribucion = parseFloat(distribucion.mora || 0) +
                                parseFloat(distribucion.interes || 0) +
                                parseFloat(distribucion.capital || 0);

      // Solo enviar distribución si es diferente a la automática
      if (totalDistribucion > 0) {
        abonoData.distribucion = {
          mora: parseFloat(distribucion.mora || 0),
          interes: parseFloat(distribucion.interes || 0),
          capital: parseFloat(distribucion.capital || 0)
        };
      }
    }

    return abonoData;
  };

  const formatearMoneda = (monto) => {
    return new Intl.NumberFormat('es-HN', {
      style: 'currency',
      currency: 'HNL'
    }).format(monto);
  };

  const requiereDocumentoReferencia = () => {
    if (!formaPagoId) return false;
    const formaPagoSeleccionada = formasPago.find(f => f.formaPagoId === parseInt(formaPagoId));
    return formaPagoSeleccionada?.formaPagoNombre?.toLowerCase().includes('transferencia') ||
           formaPagoSeleccionada?.formaPagoNombre?.toLowerCase().includes('cheque');
  };

  return (
    <div className="transaccion-abono-form">
      <div className="form-header">
        <h2>Abono a Préstamo</h2>
        <button
          type="button"
          className="btn-ver-estado-cuenta"
          onClick={() => setMostrarEstadoCuenta(true)}
          disabled={cargandoEstadoCuenta}
        >
          Ver Estado de Cuenta
        </button>
      </div>

      {/* Información del Préstamo */}
      <div className="prestamo-info-card">
        <div className="info-row">
          <div className="info-col">
            <label>Número de Préstamo:</label>
            <span className="valor-destacado">{prestamo.prestamoNumero}</span>
          </div>
          <div className="info-col">
            <label>Cliente:</label>
            <span>{prestamo.nombreCliente}</span>
          </div>
        </div>

        {estadoCuenta && (
          <div className="saldos-resumen">
            <div className="saldo-item">
              <label>Saldo Capital:</label>
              <span className="saldo-valor">{formatearMoneda(estadoCuenta.saldoCapital)}</span>
            </div>
            <div className="saldo-item">
              <label>Saldo Interés:</label>
              <span className="saldo-valor">{formatearMoneda(estadoCuenta.saldoInteres)}</span>
            </div>
            <div className="saldo-item mora">
              <label>Saldo Mora:</label>
              <span className="saldo-valor">{formatearMoneda(estadoCuenta.saldoMora)}</span>
            </div>
            <div className="saldo-item total">
              <label>Saldo Total:</label>
              <span className="saldo-valor-total">{formatearMoneda(estadoCuenta.saldoTotal)}</span>
            </div>
          </div>
        )}
      </div>

      {/* Formulario */}
      <form onSubmit={handleSubmit} className="abono-form">
        {error && (
          <div className="mensaje-error">
            {error}
          </div>
        )}

        {/* Tipo de Abono */}
        <div className="form-group">
          <label htmlFor="tipoAbono">Tipo de Abono *</label>
          <select
            id="tipoAbono"
            value={tipoAbono}
            onChange={(e) => setTipoAbono(e.target.value)}
            required
          >
            <option value="PARCIAL">Abono Parcial (Distribución Automática)</option>
            <option value="CUOTA">Cuota Completa</option>
            <option value="CAPITAL">Abono a Capital</option>
            <option value="MORA">Abono a Mora</option>
          </select>
          <small className="form-help">
            {tipoAbono === 'PARCIAL' && 'El sistema distribuirá el monto automáticamente: Mora → Interés → Capital'}
            {tipoAbono === 'CUOTA' && 'Paga cuotas completas en orden de vencimiento'}
            {tipoAbono === 'CAPITAL' && 'Abono extraordinario que reduce el capital del préstamo'}
            {tipoAbono === 'MORA' && 'Pago específico para componentes de mora vencidos'}
          </small>
        </div>

        {/* Número de Cuota (solo para tipo CUOTA) */}
        {tipoAbono === 'CUOTA' && (
          <div className="form-group">
            <label htmlFor="numeroCuota">Número de Cuota</label>
            <input
              type="number"
              id="numeroCuota"
              value={numeroCuota}
              onChange={(e) => setNumeroCuota(e.target.value)}
              min="1"
              placeholder="Dejar vacío para pagar desde la primera cuota vencida"
            />
            <small className="form-help">
              Opcional: Si se especifica, pagará desde esta cuota. Si se deja vacío, pagará desde la primera cuota vencida.
            </small>
          </div>
        )}

        {/* Monto */}
        <div className="form-group">
          <label htmlFor="monto">Monto del Abono * (L)</label>
          <input
            type="number"
            id="monto"
            value={monto}
            onChange={(e) => setMonto(e.target.value)}
            step="0.01"
            min="0.01"
            required
            placeholder="0.00"
          />
        </div>

        {/* Distribución (solo para tipo PARCIAL) */}
        {tipoAbono === 'PARCIAL' && monto && (
          <div className="distribucion-section">
            <h3>Distribución del Pago</h3>
            <div className="distribucion-grid">
              <div className="form-group">
                <label htmlFor="distMora">Mora (L)</label>
                <input
                  type="number"
                  id="distMora"
                  value={distribucion.mora}
                  onChange={(e) => setDistribucion({ ...distribucion, mora: e.target.value })}
                  step="0.01"
                  min="0"
                  readOnly
                />
              </div>
              <div className="form-group">
                <label htmlFor="distInteres">Interés (L)</label>
                <input
                  type="number"
                  id="distInteres"
                  value={distribucion.interes}
                  onChange={(e) => setDistribucion({ ...distribucion, interes: e.target.value })}
                  step="0.01"
                  min="0"
                  readOnly
                />
              </div>
              <div className="form-group">
                <label htmlFor="distCapital">Capital (L)</label>
                <input
                  type="number"
                  id="distCapital"
                  value={distribucion.capital}
                  onChange={(e) => setDistribucion({ ...distribucion, capital: e.target.value })}
                  step="0.01"
                  min="0"
                  readOnly
                />
              </div>
            </div>
            <small className="form-help distribucion-ayuda">
              La distribución se calcula automáticamente siguiendo el orden: Mora → Interés → Capital
            </small>
          </div>
        )}

        {/* Forma de Pago */}
        <div className="form-group">
          <label htmlFor="formaPago">Forma de Pago *</label>
          <select
            id="formaPago"
            value={formaPagoId}
            onChange={(e) => setFormaPagoId(e.target.value)}
            required
          >
            <option value="">Seleccione una forma de pago</option>
            {formasPago.map((forma) => (
              <option key={forma.formaPagoId} value={forma.formaPagoId}>
                {forma.formaPagoNombre}
                {forma.fondo && ` → ${forma.fondo.fondoNombre}`}
              </option>
            ))}
          </select>
        </div>

        {/* Documento de Referencia (condicional) */}
        {requiereDocumentoReferencia() && (
          <div className="form-group">
            <label htmlFor="documentoReferencia">Número de Referencia *</label>
            <input
              type="text"
              id="documentoReferencia"
              value={documentoReferencia}
              onChange={(e) => setDocumentoReferencia(e.target.value)}
              required
              placeholder="Ej: 123456789"
              maxLength="50"
            />
            <small className="form-help">
              Ingrese el número de referencia de la transferencia o cheque
            </small>
          </div>
        )}

        {/* Observaciones */}
        <div className="form-group">
          <label htmlFor="observaciones">Observaciones</label>
          <textarea
            id="observaciones"
            value={observaciones}
            onChange={(e) => setObservaciones(e.target.value)}
            rows="3"
            maxLength="500"
            placeholder="Información adicional sobre el pago (opcional)"
          />
        </div>

        {/* Botones */}
        <div className="form-actions">
          <button
            type="button"
            className="btn-cancelar"
            onClick={onCancelar}
            disabled={procesando}
          >
            Cancelar
          </button>
          <button
            type="submit"
            className="btn-aplicar"
            disabled={procesando || cargandoEstadoCuenta}
          >
            {procesando ? 'Procesando...' : 'Aplicar Abono'}
          </button>
        </div>
      </form>

      {/* Modal de Estado de Cuenta */}
      {mostrarEstadoCuenta && estadoCuenta && (
        <EstadoCuentaModal
          estadoCuenta={estadoCuenta}
          onCerrar={() => setMostrarEstadoCuenta(false)}
        />
      )}
    </div>
  );
};

export default TransaccionAbonoForm;
