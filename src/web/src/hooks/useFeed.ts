import { useInfiniteQuery } from '@tanstack/react-query';
import { apiClient } from '../api/client';
import type { FeedItem, PagedResponse } from '../types';

export function useFeed(limit = 10) {
  const query = useInfiniteQuery({
    queryKey: ['feed'],
    queryFn: ({ pageParam }) => {
      const params = new URLSearchParams({ limit: String(limit) });
      if (pageParam) params.set('cursor', pageParam);
      return apiClient<PagedResponse<FeedItem>>(`/feed?${params}`);
    },
    initialPageParam: '' as string,
    getNextPageParam: (lastPage) => (lastPage.hasMore ? lastPage.cursor : undefined),
  });

  const items = query.data?.pages.flatMap((page) => page.data) ?? [];

  return {
    items,
    isLoading: query.isLoading,
    isSuccess: query.isSuccess,
    isError: query.isError,
    error: query.error,
    hasNextPage: query.hasNextPage,
    fetchNextPage: query.fetchNextPage,
    isFetchingNextPage: query.isFetchingNextPage,
  };
}
