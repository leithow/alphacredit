import React from 'react';
import DniUploader from './DniUploader';
import './DniModal.css';

const DniModal = ({ isOpen, onClose, personaId, personaNombre }) => {
  if (!isOpen) return null;

  const handleOverlayClick = (e) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <div className="dni-modal-overlay" onClick={handleOverlayClick}>
      <div className="dni-modal-content">
        <div className="dni-modal-header">
          <h2>📸 Documentos de Identidad</h2>
          {personaNombre && <p className="persona-nombre">{personaNombre}</p>}
          <button className="dni-modal-close" onClick={onClose}>
            ✕
          </button>
        </div>
        <div className="dni-modal-body">
          <DniUploader personaId={personaId} />
        </div>
        <div className="dni-modal-footer">
          <button className="btn-secondary" onClick={onClose}>
            Cerrar
          </button>
        </div>
      </div>
    </div>
  );
};

export default DniModal;
