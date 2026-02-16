import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useOrgDashboard } from '../hooks/useOrgDashboard';
import RelativeTime from '../components/shared/RelativeTime';
import type { InterestSummary, OrgDashboardStats, PostSummary, SetupChecklist } from '../types';

export default function OrgDashboardPage() {
  const { data, isLoading, isError, error } = useOrgDashboard();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-cream">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-coral border-t-transparent" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-cream px-4">
        <div className="rounded-2xl bg-white p-8 text-center shadow-sm max-w-md w-full">
          <h2 className="font-display text-xl text-navy mb-2">Something went wrong</h2>
          <p className="font-body text-navy/60 text-sm">{error?.message || 'Please try again later.'}</p>
        </div>
      </div>
    );
  }

  if (!data) return null;

  // Pending verification state
  if (data.verificationStatus === 'pending') {
    return (
      <div className="flex items-center justify-center min-h-screen bg-cream px-4">
        <div className="rounded-2xl bg-white p-8 text-center shadow-sm max-w-md w-full">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-cream">
            <svg
              className="h-8 w-8 text-teal"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
          </div>
          <h1 className="font-display text-2xl text-navy mb-2">Your organization is under review</h1>
          <p className="font-body text-navy/60 text-sm">
            We are reviewing your application and will email you within 48 hours. Thank you for your
            patience.
          </p>
        </div>
      </div>
    );
  }

  // Rejected state
  if (data.verificationStatus === 'rejected') {
    return (
      <div className="flex items-center justify-center min-h-screen bg-cream px-4">
        <div className="rounded-2xl bg-white p-8 text-center shadow-sm max-w-md w-full">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-red-50">
            <svg
              className="h-8 w-8 text-red-500"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z"
              />
            </svg>
          </div>
          <h1 className="font-display text-2xl text-navy mb-2">Verification not approved</h1>
          <p className="font-body text-navy/60 text-sm">
            Unfortunately, your organization verification was not approved. Please contact support
            for more information.
          </p>
        </div>
      </div>
    );
  }

  // Verified state - full dashboard
  return (
    <div className="min-h-screen bg-cream px-4 pt-6 pb-24">
      <div className="mx-auto max-w-lg">
        {/* Welcome header */}
        <h1 className="font-display text-2xl text-navy mb-6">
          Welcome back, {data.organizationName}
        </h1>

        {/* Stats cards */}
        {data.stats && <StatsCards stats={data.stats} />}

        {/* Setup checklist */}
        {data.setupChecklist && <SetupChecklistCard checklist={data.setupChecklist} />}

        {/* Recent Interests */}
        <RecentInterestsSection interests={data.recentInterests} />

        {/* Recent Posts */}
        <RecentPostsSection posts={data.recentPosts} />
      </div>
    </div>
  );
}

function StatsCards({ stats }: { stats: OrgDashboardStats }) {

  const cards = [
    { label: 'New Interests', value: stats.newInterestCount, href: '/org/content' },
    { label: 'Active Opportunities', value: stats.activeOpportunityCount, href: '/org/content' },
    { label: 'Followers', value: stats.followerCount, href: '/org/content' },
  ];

  return (
    <div className="grid grid-cols-3 gap-3 mb-6">
      {cards.map((card) => (
        <Link
          key={card.label}
          to={card.href}
          className="rounded-xl bg-white p-4 text-center shadow-sm transition-shadow hover:shadow-md"
        >
          <p className="font-body text-2xl font-bold text-navy">{card.value}</p>
          <p className="font-body text-xs text-navy/60 mt-1">{card.label}</p>
        </Link>
      ))}
    </div>
  );
}

function SetupChecklistCard({ checklist }: { checklist: SetupChecklist }) {
  const [dismissed, setDismissed] = useState(false);

  const allComplete = checklist.hasCoverImage && checklist.hasOpportunity && checklist.hasPost;
  if (allComplete || dismissed) return null;

  const items = [
    {
      done: checklist.hasCoverImage,
      label: 'Upload cover image',
      href: '/org/content',
    },
    {
      done: checklist.hasOpportunity,
      label: 'Create first opportunity',
      href: '/org/create',
    },
    {
      done: checklist.hasPost,
      label: 'Create first post',
      href: '/org/create',
    },
  ];

  return (
    <div className="mb-6 rounded-xl bg-white shadow-sm border-l-4 border-teal overflow-hidden">
      <div className="flex items-center justify-between px-4 pt-4 pb-2">
        <h2 className="font-display text-base text-navy">Get started</h2>
        <button
          onClick={() => setDismissed(true)}
          className="font-body text-xs text-navy/40 hover:text-navy/60"
        >
          Dismiss
        </button>
      </div>
      <ul className="px-4 pb-4 space-y-3">
        {items.map((item) => (
          <li key={item.label} className="flex items-center gap-3">
            {item.done ? (
              <svg
                className="h-5 w-5 text-teal flex-shrink-0"
                fill="currentColor"
                viewBox="0 0 20 20"
              >
                <path
                  fillRule="evenodd"
                  d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                  clipRule="evenodd"
                />
              </svg>
            ) : (
              <div className="h-5 w-5 rounded-full border-2 border-navy/20 flex-shrink-0" />
            )}
            {item.done ? (
              <span className="font-body text-sm text-navy/40 line-through">{item.label}</span>
            ) : (
              <Link to={item.href} className="font-body text-sm text-teal hover:underline">
                {item.label}
              </Link>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}

function RecentInterestsSection({ interests }: { interests: InterestSummary[] }) {
  return (
    <section className="mb-6">
      <div className="flex items-center justify-between mb-3">
        <h2 className="font-display text-lg text-navy">Recent Interests</h2>
        <Link to="/org/content" className="font-body text-sm text-teal hover:underline">
          View All
        </Link>
      </div>
      {interests.length === 0 ? (
        <div className="rounded-xl bg-white p-6 text-center shadow-sm">
          <p className="font-body text-sm text-navy/60">No volunteer interests yet</p>
        </div>
      ) : (
        <div className="space-y-2">
          {interests.map((interest) => (
            <div
              key={interest.interestId}
              className="flex items-center gap-3 rounded-xl bg-white p-3 shadow-sm"
            >
              {/* Avatar */}
              <div className="h-10 w-10 flex-shrink-0 rounded-full bg-cream overflow-hidden">
                {interest.volunteerAvatarUrl ? (
                  <img
                    src={interest.volunteerAvatarUrl}
                    alt={interest.volunteerName}
                    className="h-full w-full object-cover"
                  />
                ) : (
                  <div className="flex h-full w-full items-center justify-center font-body text-sm font-bold text-navy/40">
                    {interest.volunteerName.charAt(0).toUpperCase()}
                  </div>
                )}
              </div>

              {/* Content */}
              <div className="min-w-0 flex-1">
                <p className="font-body text-sm text-navy truncate">
                  <span className="font-semibold">{interest.volunteerName}</span>
                </p>
                <p className="font-body text-xs text-navy/60 truncate">
                  {interest.opportunityTitle}
                </p>
              </div>

              {/* Meta */}
              <div className="flex flex-col items-end gap-1 flex-shrink-0">
                <span
                  className={`inline-block rounded-full px-2 py-0.5 font-body text-xs font-medium ${
                    interest.status === 'pending'
                      ? 'bg-amber-50 text-amber-700'
                      : interest.status === 'accepted'
                        ? 'bg-green-50 text-green-700'
                        : 'bg-red-50 text-red-700'
                  }`}
                >
                  {interest.status}
                </span>
                <span className="font-body text-xs text-navy/40">
                  <RelativeTime date={interest.createdAt} />
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}

function RecentPostsSection({ posts }: { posts: PostSummary[] }) {
  return (
    <section className="mb-6">
      <div className="flex items-center justify-between mb-3">
        <h2 className="font-display text-lg text-navy">Recent Posts</h2>
        <Link to="/org/content" className="font-body text-sm text-teal hover:underline">
          View All
        </Link>
      </div>
      {posts.length === 0 ? (
        <div className="rounded-xl bg-white p-6 text-center shadow-sm">
          <p className="font-body text-sm text-navy/60">Create your first post to get started</p>
          <Link
            to="/org/create"
            className="mt-3 inline-block font-body text-sm text-teal hover:underline"
          >
            Create Post
          </Link>
        </div>
      ) : (
        <div className="flex gap-3 overflow-x-auto pb-2 -mx-4 px-4 snap-x snap-mandatory">
          {posts.map((post) => (
            <div
              key={post.postId}
              className="flex-shrink-0 w-40 snap-start rounded-xl bg-white shadow-sm overflow-hidden"
            >
              {/* Thumbnail */}
              <div className="h-24 bg-cream">
                {post.thumbnailUrl ? (
                  <img
                    src={post.thumbnailUrl}
                    alt={post.title}
                    className="h-full w-full object-cover"
                  />
                ) : (
                  <div className="flex h-full w-full items-center justify-center">
                    <svg
                      className="h-8 w-8 text-navy/20"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                      strokeWidth={1.5}
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M2.25 15.75l5.159-5.159a2.25 2.25 0 013.182 0l5.159 5.159m-1.5-1.5l1.409-1.409a2.25 2.25 0 013.182 0l2.909 2.909M3.75 21h16.5A2.25 2.25 0 0022.5 18.75V5.25A2.25 2.25 0 0020.25 3H3.75A2.25 2.25 0 001.5 5.25v13.5A2.25 2.25 0 003.75 21z"
                      />
                    </svg>
                  </div>
                )}
              </div>
              {/* Info */}
              <div className="p-3">
                <p className="font-body text-sm text-navy truncate">{post.title}</p>
                <p className="font-body text-xs text-navy/40 mt-1">
                  {post.viewCount} {post.viewCount === 1 ? 'view' : 'views'}
                </p>
              </div>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
