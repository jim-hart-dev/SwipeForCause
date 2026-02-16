import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import OrgDashboardPage from './OrgDashboardPage';
import type { OrgDashboardResponse } from '../types';

// Mock the hook
vi.mock('../hooks/useOrgDashboard', () => ({
  useOrgDashboard: vi.fn(),
}));

import { useOrgDashboard } from '../hooks/useOrgDashboard';
const mockUseOrgDashboard = vi.mocked(useOrgDashboard);

function renderPage() {
  return render(
    <MemoryRouter>
      <OrgDashboardPage />
    </MemoryRouter>,
  );
}

function makeDashboardData(overrides: Partial<OrgDashboardResponse> = {}): OrgDashboardResponse {
  return {
    organizationId: '123',
    organizationName: 'Test Org',
    verificationStatus: 'verified',
    stats: { newInterestCount: 5, activeOpportunityCount: 3, followerCount: 42 },
    recentInterests: [],
    recentPosts: [],
    setupChecklist: { hasCoverImage: true, hasOpportunity: true, hasPost: true },
    ...overrides,
  };
}

describe('OrgDashboardPage', () => {
  it('shows loading spinner while fetching', () => {
    mockUseOrgDashboard.mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
      error: null,
    } as ReturnType<typeof useOrgDashboard>);

    renderPage();
    expect(document.querySelector('.animate-spin')).toBeInTheDocument();
  });

  it('shows pending state for unverified orgs', () => {
    mockUseOrgDashboard.mockReturnValue({
      data: makeDashboardData({ verificationStatus: 'pending', stats: null, setupChecklist: null }),
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useOrgDashboard>);

    renderPage();
    expect(screen.getByText('Your organization is under review')).toBeInTheDocument();
    expect(screen.getByText(/email you within 48 hours/)).toBeInTheDocument();
  });

  it('shows rejected state for rejected orgs', () => {
    mockUseOrgDashboard.mockReturnValue({
      data: makeDashboardData({
        verificationStatus: 'rejected',
        stats: null,
        setupChecklist: null,
      }),
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useOrgDashboard>);

    renderPage();
    expect(screen.getByText('Verification not approved')).toBeInTheDocument();
  });

  it('shows full dashboard for verified orgs with stats', () => {
    mockUseOrgDashboard.mockReturnValue({
      data: makeDashboardData(),
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useOrgDashboard>);

    renderPage();
    expect(screen.getByText('Welcome back, Test Org')).toBeInTheDocument();
    expect(screen.getByText('5')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
    expect(screen.getByText('42')).toBeInTheDocument();
    expect(screen.getByText('New Interests')).toBeInTheDocument();
    expect(screen.getByText('Active Opportunities')).toBeInTheDocument();
    expect(screen.getByText('Followers')).toBeInTheDocument();
  });

  it('shows setup checklist when items are incomplete', () => {
    mockUseOrgDashboard.mockReturnValue({
      data: makeDashboardData({
        setupChecklist: { hasCoverImage: false, hasOpportunity: false, hasPost: true },
      }),
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useOrgDashboard>);

    renderPage();
    expect(screen.getByText('Get started')).toBeInTheDocument();
    expect(screen.getByText('Upload cover image')).toBeInTheDocument();
    expect(screen.getByText('Create first opportunity')).toBeInTheDocument();
  });

  it('hides setup checklist when all items are complete', () => {
    mockUseOrgDashboard.mockReturnValue({
      data: makeDashboardData({
        setupChecklist: { hasCoverImage: true, hasOpportunity: true, hasPost: true },
      }),
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useOrgDashboard>);

    renderPage();
    expect(screen.queryByText('Get started')).not.toBeInTheDocument();
  });

  it('shows empty states when no interests or posts', () => {
    mockUseOrgDashboard.mockReturnValue({
      data: makeDashboardData({ recentInterests: [], recentPosts: [] }),
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useOrgDashboard>);

    renderPage();
    expect(screen.getByText('No volunteer interests yet')).toBeInTheDocument();
    expect(screen.getByText('Create your first post to get started')).toBeInTheDocument();
  });

  it('shows error state when fetch fails', () => {
    mockUseOrgDashboard.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      error: new Error('Network error'),
    } as ReturnType<typeof useOrgDashboard>);

    renderPage();
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText('Network error')).toBeInTheDocument();
  });
});
