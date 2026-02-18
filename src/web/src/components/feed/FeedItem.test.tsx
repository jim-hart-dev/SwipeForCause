import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import FeedItem from './FeedItem';
import type { FeedItem as FeedItemType } from '../../types';

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

describe('FeedItem', () => {
  it('renders background image from first media url', () => {
    render(
      <MemoryRouter>
        <FeedItem item={makeFeedItem()} isActive={false} />
      </MemoryRouter>,
    );
    const img = screen.getByRole('img', { name: 'Beach Cleanup Day' });
    expect(img).toHaveAttribute('src', 'https://cdn.example.com/beach.jpg');
  });

  it('renders the overlay with post data', () => {
    render(
      <MemoryRouter>
        <FeedItem item={makeFeedItem()} isActive={false} />
      </MemoryRouter>,
    );
    expect(screen.getByText('Beach Cleanup Day')).toBeInTheDocument();
    expect(screen.getByText('Ocean Guardians')).toBeInTheDocument();
  });

  it('uses thumbnail when available for video posts', () => {
    render(
      <MemoryRouter>
        <FeedItem
          item={makeFeedItem({
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
          })}
          isActive={false}
        />
      </MemoryRouter>,
    );
    const img = screen.getByRole('img', { name: 'Beach Cleanup Day' });
    expect(img).toHaveAttribute('src', 'https://cdn.example.com/video-thumb.jpg');
  });
});
