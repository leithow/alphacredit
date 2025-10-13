import { useState, useEffect } from 'react';
import logo from './logo.svg';
import './App.css';

function App() {
  const [healthStatus, setHealthStatus] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const checkHealth = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch('http://localhost:5000/api/health');
      if (!response.ok) {
        throw new Error('Failed to fetch health status');
      }
      const data = await response.json();
      setHealthStatus(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <h1>AlphaCredit</h1>
        <p>Backend + Frontend Integration</p>

        <button onClick={checkHealth} disabled={loading}>
          {loading ? 'Checking...' : 'Check Backend Health'}
        </button>

        {error && (
          <div style={{ color: '#ff6b6b', marginTop: '20px' }}>
            <strong>Error:</strong> {error}
          </div>
        )}

        {healthStatus && (
          <div style={{ marginTop: '20px', textAlign: 'left', background: '#282c34', padding: '20px', borderRadius: '8px' }}>
            <h3>Backend Status:</h3>
            <p><strong>Status:</strong> {healthStatus.status}</p>
            <p><strong>Service:</strong> {healthStatus.service}</p>
            <p><strong>Timestamp:</strong> {new Date(healthStatus.timestamp).toLocaleString()}</p>
          </div>
        )}
      </header>
    </div>
  );
}

export default App;
