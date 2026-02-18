import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import FeedItem from './FeedItem';
import { MuteProvider } from '../../contexts/MuteContext';
import type { FeedItem as FeedItemType } from '../../types';

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

function makeFeedItem(overrides: Partial<FeedItemType> = {}): FeedItemType {
  return {
    postId: 'post-1',
    title: 'Beach Cleanup Day',
    description: 'Join us for a fun day cleaning up the local beach.',
    mediaType: 'image',
    createdAt: '2026-01-01T00:00:00Z',
    media: [
      {
        id: 'media-1',
        url: 'https://cdn.example.com/beach.jpg',
        thumbnailUrl: 'https://cdn.example.com/beach-thumb.jpg',
        duration: null,
        width: 1080,
        height: 1920,
      },
    ],
    organization: {
      id: 'org-1',
      name: 'Ocean Guardians',
      logoUrl: 'https://cdn.example.com/logo.jpg',
      isVerified: true,
    },
    opportunity: null,
    ...overrides,
  };
}

function renderFeedItem(item: FeedItemType, isActive = false) {
  return render(
    <MemoryRouter>
      <MuteProvider>
        <FeedItem item={item} isActive={isActive} />
      </MuteProvider>
    </MemoryRouter>,
  );
}

describe('FeedItem', () => {
  it('renders background image from first media url for image posts', () => {
    renderFeedItem(makeFeedItem());
    const img = screen.getByRole('img', { name: 'Beach Cleanup Day' });
    expect(img).toHaveAttribute('src', 'https://cdn.example.com/beach.jpg');
  });

  it('renders the overlay with post data', () => {
    renderFeedItem(makeFeedItem());
    expect(screen.getByText('Beach Cleanup Day')).toBeInTheDocument();
    expect(screen.getByText('Ocean Guardians')).toBeInTheDocument();
  });

  it('renders VideoPlayer for video posts', () => {
    renderFeedItem(
      makeFeedItem({
        mediaType: 'video',
        media: [
          {
            id: 'media-1',
            url: 'https://cdn.example.com/video.mp4',
            thumbnailUrl: 'https://cdn.example.com/video-thumb.jpg',
            duration: 30,
            width: 1080,
            height: 1920,
          },
        ],
      }),
      true,
    );
    expect(screen.getByTestId('video-player')).toBeInTheDocument();
  });

  it('renders VideoPlayer for video posts even when inactive', () => {
    renderFeedItem(
      makeFeedItem({
        mediaType: 'video',
        media: [
          {
            id: 'media-1',
            url: 'https://cdn.example.com/video.mp4',
            thumbnailUrl: 'https://cdn.example.com/video-thumb.jpg',
            duration: 30,
            width: 1080,
            height: 1920,
          },
        ],
      }),
      false,
    );
    expect(screen.getByTestId('video-player')).toBeInTheDocument();
  });
});
