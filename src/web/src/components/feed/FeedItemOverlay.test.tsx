import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import FeedItemOverlay from './FeedItemOverlay';
import type { FeedItem } from '../../types';

function makeFeedItem(overrides: Partial<FeedItem> = {}): FeedItem {
  return {
    postId: 'post-1',
    title: 'Beach Cleanup Day',
    description: 'Join us for a fun day cleaning up the local beach.',
    mediaType: 'image',
    createdAt: '2026-01-01T00:00:00Z',
    media: [],
    organization: {
      id: 'org-1',
      name: 'Ocean Guardians',
      logoUrl: 'https://cdn.example.com/logo.jpg',
      isVerified: true,
    },
    opportunity: {
      id: 'opp-1',
      title: 'Beach Cleanup',
      scheduleType: 'one_time',
      startDate: '2026-02-01T00:00:00Z',
      location: 'Santa Monica, CA',
      isRemote: false,
      timeCommitment: '3 hours',
    },
    ...overrides,
  };
}

function renderOverlay(item: FeedItem) {
  return render(
    <MemoryRouter>
      <FeedItemOverlay item={item} />
    </MemoryRouter>,
  );
}

describe('FeedItemOverlay', () => {
  it('renders organization name and logo', () => {
    renderOverlay(makeFeedItem());
    expect(screen.getByText('Ocean Guardians')).toBeInTheDocument();
    expect(screen.getByRole('img', { name: 'Ocean Guardians logo' })).toHaveAttribute(
      'src',
      'https://cdn.example.com/logo.jpg',
    );
  });

  it('renders verified badge when org is verified', () => {
    renderOverlay(makeFeedItem());
    expect(screen.getByLabelText('Verified organization')).toBeInTheDocument();
  });

  it('does not render verified badge when org is not verified', () => {
    renderOverlay(
      makeFeedItem({
        organization: { id: 'org-1', name: 'Unverified Org', logoUrl: null, isVerified: false },
      }),
    );
    expect(screen.queryByLabelText('Verified organization')).not.toBeInTheDocument();
  });

  it('renders post title and description', () => {
    renderOverlay(makeFeedItem());
    expect(screen.getByText('Beach Cleanup Day')).toBeInTheDocument();
    expect(
      screen.getByText('Join us for a fun day cleaning up the local beach.'),
    ).toBeInTheDocument();
  });

  it('renders Volunteer Now CTA button', () => {
    renderOverlay(makeFeedItem());
    expect(screen.getByRole('button', { name: 'Volunteer Now' })).toBeInTheDocument();
  });

  it('renders org link pointing to org profile', () => {
    renderOverlay(makeFeedItem());
    const link = screen.getByRole('link', { name: /Ocean Guardians/ });
    expect(link).toHaveAttribute('href', '/org/org-1');
  });

  it('renders fallback initial when org has no logo', () => {
    renderOverlay(
      makeFeedItem({
        organization: { id: 'org-1', name: 'No Logo Org', logoUrl: null, isVerified: true },
      }),
    );
    expect(screen.getByText('N')).toBeInTheDocument();
    expect(screen.queryByRole('img', { name: /logo/ })).not.toBeInTheDocument();
  });
});
