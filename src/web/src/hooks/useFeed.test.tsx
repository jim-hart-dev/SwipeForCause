import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import type { ReactNode } from 'react';
import { useFeed } from './useFeed';
import type { FeedItem, PagedResponse } from '../types';

vi.mock('../api/client', () => ({
  apiClient: vi.fn(),
}));

import { apiClient } from '../api/client';

const mockApiClient = vi.mocked(apiClient);

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

function makeFeedItem(overrides: Partial<FeedItem> = {}): FeedItem {
  return {
    postId: crypto.randomUUID(),
    title: 'Test Post',
    description: 'A test post',
    mediaType: 'image',
    createdAt: '2026-01-01T00:00:00Z',
    media: [
      {
        id: crypto.randomUUID(),
        url: 'https://cdn.example.com/image.jpg',
        thumbnailUrl: null,
        duration: null,
        width: 1080,
        height: 1920,
      },
    ],
    organization: {
      id: crypto.randomUUID(),
      name: 'Test Org',
      logoUrl: null,
      isVerified: true,
    },
    opportunity: null,
    ...overrides,
  };
}

describe('useFeed', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('fetches the first page of feed items', async () => {
    const items = [makeFeedItem(), makeFeedItem()];
    mockApiClient.mockResolvedValueOnce({
      data: items,
      cursor: 'abc123',
      hasMore: true,
    } satisfies PagedResponse<FeedItem>);

    const { result } = renderHook(() => useFeed(), { wrapper: createWrapper() });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockApiClient).toHaveBeenCalledWith('/feed?limit=10');
    expect(result.current.items).toHaveLength(2);
    expect(result.current.items[0].title).toBe('Test Post');
    expect(result.current.hasNextPage).toBe(true);
  });

  it('includes cursor in subsequent page requests', async () => {
    mockApiClient.mockResolvedValueOnce({
      data: [makeFeedItem()],
      cursor: 'cursor1',
      hasMore: true,
    } satisfies PagedResponse<FeedItem>);

    const { result } = renderHook(() => useFeed(), { wrapper: createWrapper() });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    mockApiClient.mockResolvedValueOnce({
      data: [makeFeedItem()],
      cursor: 'cursor2',
      hasMore: false,
    } satisfies PagedResponse<FeedItem>);

    result.current.fetchNextPage();

    await waitFor(() => expect(result.current.items).toHaveLength(2));

    expect(mockApiClient).toHaveBeenCalledWith('/feed?limit=10&cursor=cursor1');
    expect(result.current.hasNextPage).toBe(false);
  });

  it('returns empty items array when no data', async () => {
    mockApiClient.mockResolvedValueOnce({
      data: [],
      cursor: null,
      hasMore: false,
    } satisfies PagedResponse<FeedItem>);

    const { result } = renderHook(() => useFeed(), { wrapper: createWrapper() });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.items).toHaveLength(0);
    expect(result.current.hasNextPage).toBe(false);
  });
});
