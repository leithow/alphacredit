import axiosInstance from '../api/axiosConfig';

const garantiaService = {
  async getGarantias(pageNumber = 1, pageSize = 10, filters = {}) {
    const params = {
      pageNumber,
      pageSize,
      ...filters,
    };
    const response = await axiosInstance.get('/garantias', { params });
    return response.data;
  },

  async getGarantiasByPrestamo(prestamoId, pageNumber = 1, pageSize = 10) {
    const params = { pageNumber, pageSize };
    const response = await axiosInstance.get(`/garantias/prestamo/${prestamoId}`, { params });
    return response.data;
  },

  async getGarantiaById(id) {
    const response = await axiosInstance.get(`/garantias/${id}`);
    return response.data;
  },

  async createGarantia(garantiaData) {
    const response = await axiosInstance.post('/garantias', garantiaData);
    return response.data;
  },

  async updateGarantia(id, garantiaData) {
    const response = await axiosInstance.put(`/garantias/${id}`, garantiaData);
    return response.data;
  },

  async deleteGarantia(id) {
    const response = await axiosInstance.delete(`/garantias/${id}`);
    return response.data;
  },

  async getGarantiasDisponibles(personaId = null) {
    const params = personaId ? { personaId } : {};
    const response = await axiosInstance.get('/garantias/disponibles', { params });
    return response.data;
  },
};

export default garantiaService;
