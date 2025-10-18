import React, { useState, useEffect } from 'react';
import Modal from './Modal';
import Button from './Button';
import personaService from '../../services/personaService';
import './ClienteSearchModal.css';

const ClienteSearchModal = ({ isOpen, onClose, onSelectCliente }) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [clientes, setClientes] = useState([]);
  const [filteredClientes, setFilteredClientes] = useState([]);
  const [loading, setLoading] = useState(false);
  const [selectedCliente, setSelectedCliente] = useState(null);

  useEffect(() => {
    if (isOpen) {
      loadClientes();
      setSearchTerm('');
      setSelectedCliente(null);
    }
  }, [isOpen]);

  useEffect(() => {
    // Filtrar clientes cuando cambia el término de búsqueda
    if (searchTerm.trim() === '') {
      setFilteredClientes(clientes);
    } else {
      const term = searchTerm.toLowerCase();
      const filtered = clientes.filter(
        (cliente) =>
          cliente.personaNombreCompleto.toLowerCase().includes(term) ||
          cliente.personaIdentificacion.toLowerCase().includes(term) ||
          cliente.personaEmail?.toLowerCase().includes(term)
      );
      setFilteredClientes(filtered);
    }
  }, [searchTerm, clientes]);

  const loadClientes = async () => {
    try {
      setLoading(true);
      const data = await personaService.getClientes();
      setClientes(data);
      setFilteredClientes(data);
    } catch (error) {
      console.error('Error al cargar clientes:', error);
      alert('Error al cargar la lista de clientes');
    } finally {
      setLoading(false);
    }
  };

  const handleSelectCliente = (cliente) => {
    setSelectedCliente(cliente);
  };

  const handleConfirm = () => {
    if (selectedCliente) {
      onSelectCliente(selectedCliente);
      onClose();
    } else {
      alert('Por favor seleccione un cliente');
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Buscar Cliente" size="large">
      <div className="cliente-search-modal">
        {/* Barra de búsqueda */}
        <div className="search-bar">
          <input
            type="text"
            className="search-input"
            placeholder="🔍 Buscar por nombre, identificación o email..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            autoFocus
          />
          {searchTerm && (
            <button className="clear-search" onClick={() => setSearchTerm('')}>
              ✕
            </button>
          )}
        </div>

        {/* Resultados */}
        <div className="search-results">
          {loading ? (
            <div className="loading-state">
              <div className="spinner"></div>
              <p>Cargando clientes...</p>
            </div>
          ) : filteredClientes.length === 0 ? (
            <div className="empty-state">
              <p>
                {searchTerm
                  ? `No se encontraron clientes con "${searchTerm}"`
                  : 'No hay clientes disponibles'}
              </p>
            </div>
          ) : (
            <div className="clientes-list">
              {filteredClientes.map((cliente) => (
                <div
                  key={cliente.personaId}
                  className={`cliente-item ${
                    selectedCliente?.personaId === cliente.personaId ? 'selected' : ''
                  }`}
                  onClick={() => handleSelectCliente(cliente)}
                >
                  <div className="cliente-info">
                    <div className="cliente-name">{cliente.personaNombreCompleto}</div>
                    <div className="cliente-details">
                      <span className="detail-badge">
                        {cliente.tipoIdentificacion?.tipoIdentificacionNombre}:{' '}
                        {cliente.personaIdentificacion}
                      </span>
                      {cliente.personaEmail && (
                        <span className="detail-item">✉ {cliente.personaEmail}</span>
                      )}
                      {cliente.personaDireccion && (
                        <span className="detail-item">📍 {cliente.personaDireccion}</span>
                      )}
                    </div>
                  </div>
                  <div className="cliente-action">
                    {selectedCliente?.personaId === cliente.personaId && (
                      <span className="check-icon">✓</span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Información del cliente seleccionado */}
        {selectedCliente && (
          <div className="selected-cliente-info">
            <strong>Cliente seleccionado:</strong> {selectedCliente.personaNombreCompleto} -{' '}
            {selectedCliente.personaIdentificacion}
          </div>
        )}

        {/* Acciones */}
        <div className="modal-actions">
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button variant="primary" onClick={handleConfirm} disabled={!selectedCliente}>
            Confirmar Selección
          </Button>
        </div>
      </div>
    </Modal>
  );
};

export default ClienteSearchModal;
