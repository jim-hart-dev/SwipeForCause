import { useState, useCallback } from 'react';

interface GeolocationResult {
  city: string;
  state: string;
}

interface UseGeolocationReturn {
  detect: (onSuccess: (result: GeolocationResult) => void) => void;
  isLoading: boolean;
  error: string | null;
}

export function useGeolocation(): UseGeolocationReturn {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const detect = useCallback((onSuccess: (result: GeolocationResult) => void) => {
    if (!navigator.geolocation) {
      setError('Geolocation is not supported by your browser.');
      return;
    }

    setIsLoading(true);
    setError(null);

    navigator.geolocation.getCurrentPosition(
      async (position) => {
        try {
          const { latitude, longitude } = position.coords;
          const response = await fetch(
            `https://nominatim.openstreetmap.org/reverse?format=json&lat=${latitude}&lon=${longitude}&zoom=10&addressdetails=1`,
            { headers: { 'User-Agent': 'SwipeForCause/1.0' } },
          );
          const data = await response.json();
          const address = data.address;
          const result: GeolocationResult = {
            city: address.city || address.town || address.village || '',
            state: address.state || '',
          };
          onSuccess(result);
        } catch {
          setError('Could not determine your location. Please enter it manually.');
        } finally {
          setIsLoading(false);
        }
      },
      () => {
        setError('Location access denied. Please enter your location manually.');
        setIsLoading(false);
      },
      { timeout: 10000 },
    );
  }, []);

  return { detect, isLoading, error };
}
