import axiosInstance from '../api/axiosConfig';
import { API_ENDPOINTS } from '../constants/apiEndpoints';

class CatalogService {
  /**
   * Obtiene un catálogo genérico
   * @param {string} catalogName - Nombre del catálogo
   * @param {boolean} soloActivos - Filtrar solo activos
   * @returns {Promise} Lista del catálogo
   */
  async getCatalog(catalogName, soloActivos = true) {
    try {
      const response = await axiosInstance.get(catalogName, {
        params: { soloActivos, pageSize: 1000 },
      });
      return response.data;
    } catch (error) {
      console.error(`Error al obtener catálogo ${catalogName}:`, error);
      throw error;
    }
  }

  // Métodos específicos para cada catálogo
  async getTiposIdentificacion() {
    return this.getCatalog(API_ENDPOINTS.CATALOGS.TIPO_IDENTIFICACION);
  }

  async getSexos() {
    return this.getCatalog(API_ENDPOINTS.CATALOGS.SEXO);
  }

  async getEstadosCiviles() {
    return this.getCatalog(API_ENDPOINTS.CATALOGS.ESTADO_CIVIL);
  }

  async getEstadosPrestamo() {
    return this.getCatalog(API_ENDPOINTS.CATALOGS.ESTADO_PRESTAMO);
  }

  async getFrecuenciasPago() {
    return this.getCatalog(API_ENDPOINTS.CATALOGS.FRECUENCIA_PAGO);
  }

  async getMonedas() {
    return this.getCatalog(API_ENDPOINTS.CATALOGS.MONEDA);
  }

  async getTiposGarantia() {
    return this.getCatalog(API_ENDPOINTS.CATALOGS.TIPO_GARANTIA);
  }

  async getEstadosGarantia() {
    return this.getCatalog(API_ENDPOINTS.CATALOGS.ESTADO_GARANTIA);
  }

  async getDestinosCredito() {
    return this.getCatalog(API_ENDPOINTS.CATALOGS.DESTINO_CREDITO);
  }

  async getComponentesPrestamo() {
    return this.getCatalog(API_ENDPOINTS.CATALOGS.COMPONENTE_PRESTAMO);
  }

  async getEstadosComponente() {
    return this.getCatalog(API_ENDPOINTS.CATALOGS.ESTADO_COMPONENTE);
  }

  async getTiposFondo() {
    return this.getCatalog(API_ENDPOINTS.CATALOGS.TIPO_FONDO);
  }

  async getSucursales() {
    try {
      const response = await axiosInstance.get(API_ENDPOINTS.SUCURSALES, {
        params: { pageSize: 1000 },
      });
      return response.data;
    } catch (error) {
      console.error('Error al obtener sucursales:', error);
      throw error;
    }
  }
}

export default new CatalogService();
