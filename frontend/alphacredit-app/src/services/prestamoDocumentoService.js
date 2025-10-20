import axiosInstance from '../api/axiosConfig';

const prestamoDocumentoService = {
  /**
   * Genera y obtiene el contrato de préstamo
   * @param {number} prestamoId - ID del préstamo
   * @param {string} usuario - Usuario que genera el documento
   * @returns {Promise<string>} HTML del contrato
   */
  generarContrato: async (prestamoId, usuario = 'system') => {
    const response = await axiosInstance.get(
      `/prestamos/${prestamoId}/documentos/contrato`,
      {
        params: { usuario },
        responseType: 'text',
      }
    );
    return response.data;
  },

  /**
   * Genera y obtiene el pagaré del préstamo
   * @param {number} prestamoId - ID del préstamo
   * @param {string} usuario - Usuario que genera el documento
   * @returns {Promise<string>} HTML del pagaré
   */
  generarPagare: async (prestamoId, usuario = 'system') => {
    const response = await axiosInstance.get(
      `/prestamos/${prestamoId}/documentos/pagare`,
      {
        params: { usuario },
        responseType: 'text',
      }
    );
    return response.data;
  },

  /**
   * Genera y obtiene el plan de pagos del préstamo
   * @param {number} prestamoId - ID del préstamo
   * @param {string} usuario - Usuario que genera el documento
   * @returns {Promise<string>} HTML del plan de pagos
   */
  generarPlanPagos: async (prestamoId, usuario = 'system') => {
    const response = await axiosInstance.get(
      `/prestamos/${prestamoId}/documentos/plan-pagos`,
      {
        params: { usuario },
        responseType: 'text',
      }
    );
    return response.data;
  },

  /**
   * Obtiene el historial de impresiones de un documento
   * @param {number} prestamoId - ID del préstamo
   * @param {string} tipoDocumento - Tipo de documento (CONTRATO, PAGARE, PLAN_PAGOS)
   * @returns {Promise<Object>} Historial de impresiones
   */
  obtenerHistorialImpresiones: async (prestamoId, tipoDocumento) => {
    const response = await axiosInstance.get(
      `/prestamos/${prestamoId}/documentos/historial`,
      {
        params: { tipoDocumento },
      }
    );
    return response.data;
  },

  /**
   * Obtiene las estadísticas de documentos de un préstamo
   * @param {number} prestamoId - ID del préstamo
   * @returns {Promise<Object>} Estadísticas de documentos
   */
  obtenerEstadisticas: async (prestamoId) => {
    const response = await axiosInstance.get(
      `/prestamos/${prestamoId}/documentos/estadisticas`
    );
    return response.data;
  },

  /**
   * Abre un documento en una nueva ventana para imprimir
   * @param {string} htmlContent - Contenido HTML del documento
   * @param {string} titulo - Título de la ventana
   */
  imprimirDocumento: (htmlContent, titulo = 'Documento') => {
    const ventanaImpresion = window.open('', '_blank', 'width=800,height=600');
    if (ventanaImpresion) {
      ventanaImpresion.document.write(htmlContent);
      ventanaImpresion.document.close();
      ventanaImpresion.focus();

      // Esperar a que se cargue el contenido antes de imprimir
      ventanaImpresion.onload = () => {
        setTimeout(() => {
          ventanaImpresion.print();
        }, 250);
      };
    } else {
      alert('Por favor, habilite las ventanas emergentes para imprimir documentos.');
    }
  },
};

export default prestamoDocumentoService;
