import axiosInstance from '../api/axiosConfig';

const personaDocumentoService = {
  async getDocumentos(personaId) {
    const response = await axiosInstance.get(`/personas/${personaId}/documentos`);
    return response.data;
  },

  async uploadDocumento(personaId, file, tipoDocumento = 'DNI') {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('tipoDocumento', tipoDocumento);

    const response = await axiosInstance.post(
      `/personas/${personaId}/documentos/upload`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  async uploadMultipleDocumentos(personaId, files, tipoDocumento = 'DNI') {
    const formData = new FormData();
    files.forEach((file) => {
      formData.append('files', file);
    });
    formData.append('tipoDocumento', tipoDocumento);

    const response = await axiosInstance.post(
      `/personas/${personaId}/documentos/upload-multiple`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  async downloadDocumento(personaId, documentoId) {
    const response = await axiosInstance.get(
      `/personas/${personaId}/documentos/${documentoId}/download`,
      {
        responseType: 'blob',
      }
    );
    return response.data;
  },

  async deleteDocumento(personaId, documentoId) {
    const response = await axiosInstance.delete(
      `/personas/${personaId}/documentos/${documentoId}`
    );
    return response.data;
  },

  getDocumentoUrl(personaId, documentoId) {
    return `${axiosInstance.defaults.baseURL}/personas/${personaId}/documentos/${documentoId}/download`;
  },
};

export default personaDocumentoService;
