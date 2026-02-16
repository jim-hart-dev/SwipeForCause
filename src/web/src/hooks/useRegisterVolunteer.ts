import { useMutation } from '@tanstack/react-query';
import { useApiClient } from './useApiClient';
import type { RegisterVolunteerRequest, RegisterVolunteerResponse } from '../types';

export function useRegisterVolunteer() {
  const apiClient = useApiClient();

  return useMutation({
    mutationFn: (data: RegisterVolunteerRequest) =>
      apiClient<RegisterVolunteerResponse>('/volunteers', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
  });
}
