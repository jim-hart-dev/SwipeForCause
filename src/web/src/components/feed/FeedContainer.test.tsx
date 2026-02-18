import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import FeedContainer from './FeedContainer';
import { MuteProvider } from '../../contexts/MuteContext';

const mockUseFeed = vi.fn();

vi.mock('../../hooks/useFeed', () => ({
  useFeed: () => mockUseFeed(),
}));

vi.mock('../../hooks/useActiveIndex', () => ({
  useActiveIndex: () => ({ activeIndex: 0, setItemRef: vi.fn() }),
}));

beforeEach(() => {
  Object.defineProperty(HTMLVideoElement.prototype, 'play', {
    configurable: true,
    value: vi.fn().mockResolvedValue(undefined),
  });
  Object.defineProperty(HTMLVideoElement.prototype, 'pause', {
    configurable: true,
    value: vi.fn(),
  });
});

describe('FeedContainer', () => {
  it('renders skeleton while loading', () => {
    mockUseFeed.mockReturnValue({
      items: [],
      isLoading: true,
      isSuccess: false,
      isError: false,
      error: null,
      hasNextPage: false,
      fetchNextPage: vi.fn(),
      isFetchingNextPage: false,
    });

    render(
      <MemoryRouter>
        <MuteProvider>
          <FeedContainer />
        </MuteProvider>
      </MemoryRouter>,
    );

    expect(screen.getByTestId('feed-skeleton')).toBeInTheDocument();
  });

  it('renders empty state when no items', () => {
    mockUseFeed.mockReturnValue({
      items: [],
      isLoading: false,
      isSuccess: true,
      isError: false,
      error: null,
      hasNextPage: false,
      fetchNextPage: vi.fn(),
      isFetchingNextPage: false,
    });

    render(
      <MemoryRouter>
        <MuteProvider>
          <FeedContainer />
        </MuteProvider>
      </MemoryRouter>,
    );

    expect(screen.getByText(/follow organizations/i)).toBeInTheDocument();
  });

  it('renders feed items when data is available', () => {
    mockUseFeed.mockReturnValue({
      items: [
        {
          postId: 'post-1',
          title: 'Post One',
          description: null,
          mediaType: 'image',
          createdAt: '2026-01-01T00:00:00Z',
          media: [
            {
              id: 'm1',
              url: 'https://cdn.example.com/1.jpg',
              thumbnailUrl: null,
              duration: null,
              width: 1080,
              height: 1920,
            },
          ],
          organization: { id: 'org-1', name: 'Org One', logoUrl: null, isVerified: true },
          opportunity: null,
        },
        {
          postId: 'post-2',
          title: 'Post Two',
          description: null,
          mediaType: 'image',
          createdAt: '2026-01-01T00:00:00Z',
          media: [
            {
              id: 'm2',
              url: 'https://cdn.example.com/2.jpg',
              thumbnailUrl: null,
              duration: null,
              width: 1080,
              height: 1920,
            },
          ],
          organization: { id: 'org-2', name: 'Org Two', logoUrl: null, isVerified: false },
          opportunity: null,
        },
      ],
      isLoading: false,
      isSuccess: true,
      isError: false,
      error: null,
      hasNextPage: true,
      fetchNextPage: vi.fn(),
      isFetchingNextPage: false,
    });

    render(
      <MemoryRouter>
        <MuteProvider>
          <FeedContainer />
        </MuteProvider>
      </MemoryRouter>,
    );

    expect(screen.getByText('Post One')).toBeInTheDocument();
    expect(screen.getByText('Post Two')).toBeInTheDocument();
  });

  it('renders error state with retry button', () => {
    const fetchNextPage = vi.fn();
    mockUseFeed.mockReturnValue({
      items: [],
      isLoading: false,
      isSuccess: false,
      isError: true,
      error: new Error('Network error'),
      hasNextPage: false,
      fetchNextPage,
      isFetchingNextPage: false,
    });

    render(
      <MemoryRouter>
        <MuteProvider>
          <FeedContainer />
        </MuteProvider>
      </MemoryRouter>,
    );

    expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument();
  });

  it('preloads next video when current item is active', () => {
    mockUseFeed.mockReturnValue({
      items: [
        {
          postId: 'post-1',
          title: 'Post One',
          description: null,
          mediaType: 'video',
          createdAt: '2026-01-01T00:00:00Z',
          media: [
            { id: 'm1', url: 'https://cdn.example.com/1.mp4', thumbnailUrl: null, duration: 30, width: 1080, height: 1920 },
          ],
          organization: { id: 'org-1', name: 'Org One', logoUrl: null, isVerified: true },
          opportunity: null,
        },
        {
          postId: 'post-2',
          title: 'Post Two',
          description: null,
          mediaType: 'video',
          createdAt: '2026-01-01T00:00:00Z',
          media: [
            { id: 'm2', url: 'https://cdn.example.com/2.mp4', thumbnailUrl: null, duration: 30, width: 1080, height: 1920 },
          ],
          organization: { id: 'org-2', name: 'Org Two', logoUrl: null, isVerified: false },
          opportunity: null,
        },
      ],
      isLoading: false,
      isSuccess: true,
      isError: false,
      error: null,
      hasNextPage: true,
      fetchNextPage: vi.fn(),
      isFetchingNextPage: false,
    });

    render(
      <MemoryRouter>
        <MuteProvider>
          <FeedContainer />
        </MuteProvider>
      </MemoryRouter>,
    );

    const preloadLink = document.querySelector('link[rel="preload"][href="https://cdn.example.com/2.mp4"]');
    expect(preloadLink).not.toBeNull();
  });
});
