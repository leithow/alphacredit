import React, { useState, useEffect } from 'react';
import { MapContainer, TileLayer, Marker, useMapEvents } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';
import Modal from './Modal';
import Button from './Button';
import './MapModal.css';

// Fix for default marker icon in Leaflet with Webpack
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
  iconUrl: require('leaflet/dist/images/marker-icon.png'),
  shadowUrl: require('leaflet/dist/images/marker-shadow.png'),
});

const LocationMarker = ({ position, setPosition }) => {
  useMapEvents({
    click(e) {
      setPosition([e.latlng.lat, e.latlng.lng]);
    },
  });

  return position ? <Marker position={position} /> : null;
};

const MapModal = ({ isOpen, onClose, onSelectLocation, initialLocation }) => {
  const [position, setPosition] = useState(null);
  const [loading, setLoading] = useState(false);
  const [address, setAddress] = useState('');

  // República Dominicana centro (Santo Domingo)
  const defaultCenter = [18.4861, -69.9312];

  useEffect(() => {
    if (isOpen && initialLocation) {
      const [lat, lng] = initialLocation.split(',').map((coord) => parseFloat(coord.trim()));
      if (!isNaN(lat) && !isNaN(lng)) {
        setPosition([lat, lng]);
      }
    } else if (isOpen && !initialLocation) {
      // Intentar obtener ubicación actual del usuario
      getCurrentLocation();
    }
  }, [isOpen, initialLocation]);

  const getCurrentLocation = () => {
    setLoading(true);
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          setPosition([pos.coords.latitude, pos.coords.longitude]);
          setLoading(false);
        },
        (error) => {
          console.error('Error obteniendo ubicación:', error);
          setPosition(defaultCenter);
          setLoading(false);
        },
        {
          enableHighAccuracy: true,
          timeout: 10000,
          maximumAge: 0,
        }
      );
    } else {
      setPosition(defaultCenter);
      setLoading(false);
    }
  };

  const handleConfirm = () => {
    if (position) {
      const lat = position[0].toFixed(6);
      const lng = position[1].toFixed(6);
      onSelectLocation(`${lat},${lng}`);
      onClose();
    } else {
      alert('Por favor, seleccione una ubicación en el mapa');
    }
  };

  const handleUseCurrentLocation = () => {
    getCurrentLocation();
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Seleccionar Ubicación" size="large">
      <div className="map-modal-content">
        <div className="map-instructions">
          <p>📍 Haga clic en el mapa para seleccionar la ubicación exacta de la persona</p>
          {position && (
            <div className="coordinates-display">
              <strong>Coordenadas seleccionadas:</strong> {position[0].toFixed(6)},{' '}
              {position[1].toFixed(6)}
            </div>
          )}
        </div>

        <div className="map-container-wrapper">
          {loading ? (
            <div className="map-loading">
              <div className="spinner"></div>
              <p>Obteniendo ubicación actual...</p>
            </div>
          ) : (
            <MapContainer
              center={position || defaultCenter}
              zoom={position ? 15 : 12}
              style={{ height: '400px', width: '100%' }}
              key={position ? `${position[0]}-${position[1]}` : 'default'}
            >
              <TileLayer
                attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
              />
              <LocationMarker position={position} setPosition={setPosition} />
            </MapContainer>
          )}
        </div>

        <div className="map-actions">
          <Button variant="outline" onClick={handleUseCurrentLocation} disabled={loading}>
            📍 Usar Mi Ubicación Actual
          </Button>
          <div className="action-buttons">
            <Button variant="secondary" onClick={onClose}>
              Cancelar
            </Button>
            <Button variant="primary" onClick={handleConfirm} disabled={!position}>
              Confirmar Ubicación
            </Button>
          </div>
        </div>
      </div>
    </Modal>
  );
};

export default MapModal;
