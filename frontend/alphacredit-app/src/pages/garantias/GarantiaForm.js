import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import garantiaService from '../../services/garantiaService';
import catalogService from '../../services/catalogService';
import Button from '../../components/common/Button';
import './GarantiaForm.css';

const GarantiaForm = () => {
  const { prestamoId, garantiaId } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);
  const [tiposGarantia, setTiposGarantia] = useState([]);

  const [formData, setFormData] = useState({
    prestamoId: prestamoId,
    tipoGarantiaId: '',
    garantiaDescripcion: '',
    garantiaValorEstimado: '',
    garantiaDocumento: '',
    garantiaObservaciones: '',
    garantiaEstaActiva: true,
  });

  useEffect(() => {
    loadCatalogs();
    if (garantiaId) {
      setIsEditMode(true);
      loadGarantia();
    }
  }, [garantiaId]);

  const loadCatalogs = async () => {
    try {
      const tipos = await catalogService.getTiposGarantia();
      setTiposGarantia(tipos);
    } catch (error) {
      console.error('Error al cargar catálogos:', error);
      alert('Error al cargar catálogos');
    }
  };

  const loadGarantia = async () => {
    try {
      setLoading(true);
      const data = await garantiaService.getGarantiaById(garantiaId);
      setFormData({
        prestamoId: data.prestamoId,
        tipoGarantiaId: data.tipoGarantiaId,
        garantiaDescripcion: data.garantiaDescripcion || '',
        garantiaValorEstimado: data.garantiaValorEstimado || '',
        garantiaDocumento: data.garantiaDocumento || '',
        garantiaObservaciones: data.garantiaObservaciones || '',
        garantiaEstaActiva: data.garantiaEstaActiva,
      });
    } catch (error) {
      console.error('Error al cargar garantía:', error);
      alert('Error al cargar la garantía');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.tipoGarantiaId || !formData.garantiaDescripcion) {
      alert('Complete todos los campos requeridos');
      return;
    }

    const dataToSend = {
      prestamoId: parseInt(prestamoId),
      tipoGarantiaId: parseInt(formData.tipoGarantiaId),
      garantiaDescripcion: formData.garantiaDescripcion.trim(),
      garantiaValorEstimado: formData.garantiaValorEstimado ? parseFloat(formData.garantiaValorEstimado) : 0,
      garantiaDocumento: formData.garantiaDocumento.trim() || null,
      garantiaObservaciones: formData.garantiaObservaciones.trim() || null,
      garantiaEstaActiva: formData.garantiaEstaActiva,
    };

    try {
      setLoading(true);
      if (isEditMode) {
        await garantiaService.updateGarantia(garantiaId, { ...dataToSend, garantiaId: parseInt(garantiaId) });
        alert('Garantía actualizada exitosamente');
      } else {
        await garantiaService.createGarantia(dataToSend);
        alert('Garantía creada exitosamente');
      }
      navigate(`/prestamos/${prestamoId}/garantias`);
    } catch (error) {
      console.error('Error al guardar garantía:', error);
      alert(
        error.response?.data?.message ||
          error.response?.data?.title ||
          'Error al guardar la garantía'
      );
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    navigate(`/prestamos/${prestamoId}/garantias`);
  };

  const getTipoGarantiaInfo = () => {
    if (!formData.tipoGarantiaId) return null;
    const tipo = tiposGarantia.find(t => t.tipoGarantiaId === parseInt(formData.tipoGarantiaId));
    return tipo?.tipoGarantiaNombre;
  };

  const tipoSeleccionado = getTipoGarantiaInfo();

  return (
    <div className="garantia-form-container">
      <div className="form-header">
        <h1>{isEditMode ? 'Editar Garantía' : 'Nueva Garantía'}</h1>
        <p className="form-subtitle">
          Complete la información de la garantía del préstamo
        </p>
      </div>

      <form onSubmit={handleSubmit} className="garantia-form">
        <div className="form-section">
          <h2>Información General</h2>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="tipoGarantiaId">
                Tipo de Garantía <span className="required">*</span>
              </label>
              <select
                id="tipoGarantiaId"
                name="tipoGarantiaId"
                value={formData.tipoGarantiaId}
                onChange={handleChange}
                required
              >
                <option value="">Seleccione...</option>
                {tiposGarantia.map((tipo) => (
                  <option key={tipo.tipoGarantiaId} value={tipo.tipoGarantiaId}>
                    {tipo.tipoGarantiaNombre}
                  </option>
                ))}
              </select>
              {tipoSeleccionado && (
                <div className={`tipo-info tipo-${tipoSeleccionado.toLowerCase()}`}>
                  {tipoSeleccionado === 'PRENDARIA' && '📦 Bienes muebles (vehículos, maquinaria, etc.)'}
                  {tipoSeleccionado === 'HIPOTECARIA' && '🏠 Bienes inmuebles (casas, terrenos, etc.)'}
                  {tipoSeleccionado === 'FIDUCIARIA' && '🤝 Garantía personal o fianza'}
                </div>
              )}
            </div>

            <div className="form-group">
              <label htmlFor="garantiaValorEstimado">Valor Estimado</label>
              <input
                type="number"
                id="garantiaValorEstimado"
                name="garantiaValorEstimado"
                value={formData.garantiaValorEstimado}
                onChange={handleChange}
                min="0"
                step="0.01"
                placeholder="0.00"
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="garantiaDescripcion">
              Descripción <span className="required">*</span>
            </label>
            <textarea
              id="garantiaDescripcion"
              name="garantiaDescripcion"
              value={formData.garantiaDescripcion}
              onChange={handleChange}
              rows="3"
              placeholder="Describa la garantía en detalle..."
              required
            />
            <small className="field-hint">
              {tipoSeleccionado === 'PRENDARIA' && 'Ej: Vehículo marca Toyota, modelo Corolla 2020, color blanco, placa ABC-123'}
              {tipoSeleccionado === 'HIPOTECARIA' && 'Ej: Casa de habitación ubicada en Bo. Los Angeles, con 150m² de construcción'}
              {tipoSeleccionado === 'FIDUCIARIA' && 'Ej: Garantía personal del Sr. Juan Pérez, con cédula 001-010180-0001K'}
              {!tipoSeleccionado && 'Proporcione una descripción detallada de la garantía'}
            </small>
          </div>

          <div className="form-group">
            <label htmlFor="garantiaDocumento">Documento/Matrícula</label>
            <input
              type="text"
              id="garantiaDocumento"
              name="garantiaDocumento"
              value={formData.garantiaDocumento}
              onChange={handleChange}
              placeholder="Número de documento, matrícula, escritura, etc."
            />
            <small className="field-hint">
              {tipoSeleccionado === 'PRENDARIA' && 'Número de matrícula del vehículo o serie del equipo'}
              {tipoSeleccionado === 'HIPOTECARIA' && 'Número de escritura o registro de propiedad'}
              {tipoSeleccionado === 'FIDUCIARIA' && 'Número de cédula del garante'}
            </small>
          </div>
        </div>

        <div className="form-section">
          <h2>Información Adicional</h2>

          <div className="form-group">
            <label htmlFor="garantiaObservaciones">Observaciones</label>
            <textarea
              id="garantiaObservaciones"
              name="garantiaObservaciones"
              value={formData.garantiaObservaciones}
              onChange={handleChange}
              rows="3"
              placeholder="Información adicional relevante..."
            />
          </div>

          <div className="form-group checkbox-group">
            <label>
              <input
                type="checkbox"
                name="garantiaEstaActiva"
                checked={formData.garantiaEstaActiva}
                onChange={handleChange}
              />
              <span>Garantía activa</span>
            </label>
          </div>
        </div>

        <div className="form-actions">
          <Button type="button" variant="secondary" onClick={handleCancel} disabled={loading}>
            Cancelar
          </Button>
          <Button type="submit" variant="primary" disabled={loading}>
            {loading ? 'Guardando...' : isEditMode ? 'Actualizar Garantía' : 'Crear Garantía'}
          </Button>
        </div>
      </form>
    </div>
  );
};

export default GarantiaForm;
