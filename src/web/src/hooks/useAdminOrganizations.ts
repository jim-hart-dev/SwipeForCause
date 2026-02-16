import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useApiClient } from './useApiClient';
import type {
  AdminOrganization,
  AdminOrganizationDetail,
  PagedResponse,
  VerifyOrganizationRequest,
  VerifyOrganizationResponse,
} from '../types';

export function useAdminOrganizations(status?: string) {
  const apiClient = useApiClient();

  return useQuery({
    queryKey: ['admin', 'organizations', status],
    queryFn: () => {
      const params = status ? `?status=${status}` : '';
      return apiClient<PagedResponse<AdminOrganization>>(`/admin/organizations${params}`);
    },
  });
}

export function useAdminOrganizationDetail(id: string | undefined) {
  const apiClient = useApiClient();

  return useQuery({
    queryKey: ['admin', 'organizations', id],
    queryFn: () => apiClient<AdminOrganizationDetail>(`/admin/organizations/${id}`),
    enabled: !!id,
  });
}

export function useVerifyOrganization() {
  const apiClient = useApiClient();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, ...data }: VerifyOrganizationRequest & { id: string }) =>
      apiClient<VerifyOrganizationResponse>(`/admin/organizations/${id}/verify`, {
        method: 'PUT',
        body: JSON.stringify(data),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'organizations'] });
    },
  });
}
