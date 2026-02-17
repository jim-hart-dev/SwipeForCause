import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useApiClient } from './useApiClient';
import type { CreateOpportunityRequest, CreateOpportunityResponse } from '../types';

export function useCreateOpportunity() {
  const apiClient = useApiClient();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateOpportunityRequest) =>
      apiClient<CreateOpportunityResponse>('/opportunities', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['org-dashboard'] });
    },
  });
}
