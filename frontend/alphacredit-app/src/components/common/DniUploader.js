import React, { useState, useEffect } from 'react';
import personaDocumentoService from '../../services/personaDocumentoService';
import Button from './Button';
import './DniUploader.css';

const DniUploader = ({ personaId, onDocumentosChange }) => {
  const [documentos, setDocumentos] = useState([]);
  const [uploading, setUploading] = useState(false);
  const [selectedFiles, setSelectedFiles] = useState([]);
  const [previewUrls, setPreviewUrls] = useState([]);

  useEffect(() => {
    if (personaId) {
      loadDocumentos();
    }
  }, [personaId]);

  const loadDocumentos = async () => {
    try {
      const docs = await personaDocumentoService.getDocumentos(personaId);
      setDocumentos(docs);
      if (onDocumentosChange) {
        onDocumentosChange(docs);
      }
    } catch (error) {
      console.error('Error al cargar documentos:', error);
    }
  };

  const handleFileSelect = (e) => {
    const files = Array.from(e.target.files);
    setSelectedFiles(files);

    // Crear URLs de preview
    const urls = files.map((file) => {
      if (file.type.startsWith('image/')) {
        return URL.createObjectURL(file);
      }
      return null;
    });
    setPreviewUrls(urls);
  };

  const handleUpload = async () => {
    if (selectedFiles.length === 0) {
      alert('Por favor seleccione al menos un archivo');
      return;
    }

    try {
      setUploading(true);

      if (selectedFiles.length === 1) {
        await personaDocumentoService.uploadDocumento(personaId, selectedFiles[0]);
      } else {
        await personaDocumentoService.uploadMultipleDocumentos(personaId, selectedFiles);
      }

      alert(`${selectedFiles.length} documento(s) subido(s) exitosamente`);
      setSelectedFiles([]);
      setPreviewUrls([]);

      // Limpiar el input
      const fileInput = document.getElementById('dni-file-input');
      if (fileInput) {
        fileInput.value = '';
      }

      await loadDocumentos();
    } catch (error) {
      console.error('Error al subir documentos:', error);
      alert(error.response?.data?.message || 'Error al subir los documentos');
    } finally {
      setUploading(false);
    }
  };

  const handleDelete = async (doc) => {
    if (!window.confirm('¿Está seguro de eliminar este documento?')) {
      return;
    }

    try {
      await personaDocumentoService.deleteDocumento(personaId, doc.id);
      alert('Documento eliminado exitosamente');
      await loadDocumentos();
    } catch (error) {
      console.error('Error al eliminar documento:', error);
      alert('Error al eliminar el documento');
    }
  };

  const handleDownload = async (doc) => {
    try {
      const blob = await personaDocumentoService.downloadDocumento(personaId, doc.id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `DNI_${personaId}_${doc.id}`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (error) {
      console.error('Error al descargar documento:', error);
      alert('Error al descargar el documento');
    }
  };

  const getFileIcon = (tipo) => {
    const ext = tipo?.toLowerCase();
    if (ext?.includes('pdf')) return '📄';
    if (ext?.includes('jpg') || ext?.includes('jpeg') || ext?.includes('png')) return '🖼️';
    return '📎';
  };

  return (
    <div className="dni-uploader">
      <div className="dni-uploader-header">
        <h4>📸 Documentos de Identidad (DNI)</h4>
        <p className="upload-hint">Suba imágenes del DNI (frente y reverso). Formatos permitidos: JPG, PNG, PDF.</p>
      </div>

      {/* Upload Section */}
      <div className="upload-section">
        <div className="file-input-wrapper">
          <input
            type="file"
            id="dni-file-input"
            multiple
            accept=".jpg,.jpeg,.png,.pdf"
            onChange={handleFileSelect}
            disabled={!personaId || uploading}
          />
          <label htmlFor="dni-file-input" className="file-input-label">
            {selectedFiles.length > 0
              ? `${selectedFiles.length} archivo(s) seleccionado(s)`
              : '📁 Seleccionar archivos'}
          </label>
        </div>

        {selectedFiles.length > 0 && (
          <Button
            variant="primary"
            onClick={handleUpload}
            disabled={uploading || !personaId}
          >
            {uploading ? '⏳ Subiendo...' : '⬆️ Subir Documentos'}
          </Button>
        )}
      </div>

      {/* Preview de archivos seleccionados */}
      {previewUrls.length > 0 && (
        <div className="file-previews">
          <h5>Vista previa:</h5>
          <div className="preview-grid">
            {previewUrls.map((url, index) => (
              <div key={index} className="preview-item">
                {url ? (
                  <img src={url} alt={`Preview ${index + 1}`} />
                ) : (
                  <div className="preview-placeholder">
                    📄 {selectedFiles[index]?.name}
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Lista de documentos existentes */}
      {documentos.length > 0 && (
        <div className="documentos-list">
          <h5>Documentos guardados:</h5>
          <div className="documentos-grid">
            {documentos.map((doc) => (
              <div key={doc.id} className="documento-card">
                <div className="documento-header">
                  <span className="file-icon">{getFileIcon(doc.personaDocumentoTipo)}</span>
                  <span className="file-type">{doc.personaDocumentoTipo || 'DNI'}</span>
                </div>
                <div className="documento-body">
                  <small className="file-date">
                    Subido: {new Date(doc.documentoFechaCreacion).toLocaleDateString()}
                  </small>
                </div>
                <div className="documento-actions">
                  <Button
                    variant="outline"
                    size="small"
                    onClick={() => handleDownload(doc)}
                  >
                    ⬇️ Descargar
                  </Button>
                  <Button
                    variant="danger"
                    size="small"
                    onClick={() => handleDelete(doc)}
                  >
                    🗑️ Eliminar
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {!personaId && (
        <div className="upload-disabled-message">
          <p>⚠️ Primero guarde la persona para poder subir documentos</p>
        </div>
      )}
    </div>
  );
};

export default DniUploader;
