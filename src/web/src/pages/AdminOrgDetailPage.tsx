import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAdminOrganizationDetail, useVerifyOrganization } from '../hooks/useAdminOrganizations';
import StatusBadge from '../components/admin/StatusBadge';

export default function AdminOrgDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: org, isLoading, error } = useAdminOrganizationDetail(id);
  const verifyMutation = useVerifyOrganization();

  const [showRejectModal, setShowRejectModal] = useState(false);
  const [rejectReason, setRejectReason] = useState('');
  const [reasonError, setReasonError] = useState('');

  const handleVerify = () => {
    if (!id) return;
    verifyMutation.mutate(
      { id, status: 'verified' },
      {
        onSuccess: () => navigate('/admin/organizations'),
      },
    );
  };

  const handleReject = () => {
    if (!rejectReason.trim()) {
      setReasonError('A reason is required when rejecting an organization.');
      return;
    }
    if (!id) return;
    setReasonError('');
    verifyMutation.mutate(
      { id, status: 'rejected', reason: rejectReason },
      {
        onSuccess: () => {
          setShowRejectModal(false);
          navigate('/admin/organizations');
        },
      },
    );
  };

  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-coral border-t-transparent" />
      </div>
    );
  }

  if (error || !org) {
    return (
      <div className="py-12 text-center">
        <p className="text-sm text-red-600">
          {error ? 'Failed to load organization details.' : 'Organization not found.'}
        </p>
        <button
          onClick={() => navigate('/admin/organizations')}
          className="mt-4 text-sm font-medium text-teal hover:underline"
        >
          Back to list
        </button>
      </div>
    );
  }

  return (
    <div>
      {/* Back link */}
      <button
        onClick={() => navigate('/admin/organizations')}
        className="mb-6 flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700"
      >
        <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
        </svg>
        Back to organizations
      </button>

      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-4">
          {org.logoUrl ? (
            <img src={org.logoUrl} alt="" className="h-16 w-16 rounded-full object-cover" />
          ) : (
            <div className="flex h-16 w-16 items-center justify-center rounded-full bg-gray-200 text-lg font-medium text-gray-500">
              {org.name.charAt(0)}
            </div>
          )}
          <div>
            <h1 className="font-display text-2xl font-semibold text-navy">{org.name}</h1>
            <p className="text-sm text-gray-500">
              Registered {new Date(org.createdAt).toLocaleDateString()}
            </p>
          </div>
        </div>
        <StatusBadge status={org.verificationStatus} size="md" />
      </div>

      {/* Details card */}
      <div className="mt-8 rounded-lg border border-gray-200 bg-white p-6">
        <h2 className="font-display text-lg font-semibold text-navy">Organization Details</h2>

        <dl className="mt-4 grid grid-cols-1 gap-x-8 gap-y-4 sm:grid-cols-2">
          <div>
            <dt className="text-xs font-medium uppercase tracking-wider text-gray-500">EIN</dt>
            <dd className="mt-1 text-sm text-gray-900">{org.ein}</dd>
          </div>
          <div>
            <dt className="text-xs font-medium uppercase tracking-wider text-gray-500">
              Contact Name
            </dt>
            <dd className="mt-1 text-sm text-gray-900">{org.contactName}</dd>
          </div>
          <div>
            <dt className="text-xs font-medium uppercase tracking-wider text-gray-500">
              Contact Email
            </dt>
            <dd className="mt-1 text-sm text-gray-900">{org.contactEmail}</dd>
          </div>
          <div>
            <dt className="text-xs font-medium uppercase tracking-wider text-gray-500">Website</dt>
            <dd className="mt-1 text-sm">
              {org.websiteUrl ? (
                <a
                  href={org.websiteUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-teal hover:underline"
                >
                  {org.websiteUrl}
                </a>
              ) : (
                <span className="text-gray-400">--</span>
              )}
            </dd>
          </div>
          <div>
            <dt className="text-xs font-medium uppercase tracking-wider text-gray-500">
              Location
            </dt>
            <dd className="mt-1 text-sm text-gray-900">
              {org.city && org.state
                ? `${org.city}, ${org.state}`
                : org.city || org.state || '--'}
            </dd>
          </div>
          {org.verifiedAt && (
            <div>
              <dt className="text-xs font-medium uppercase tracking-wider text-gray-500">
                Verified At
              </dt>
              <dd className="mt-1 text-sm text-gray-900">
                {new Date(org.verifiedAt).toLocaleString()}
              </dd>
            </div>
          )}
        </dl>

        {/* Description */}
        <div className="mt-6">
          <h3 className="text-xs font-medium uppercase tracking-wider text-gray-500">
            Description
          </h3>
          <p className="mt-2 text-sm leading-relaxed text-gray-700">{org.description}</p>
        </div>

        {/* Categories */}
        {org.categories.length > 0 && (
          <div className="mt-6">
            <h3 className="text-xs font-medium uppercase tracking-wider text-gray-500">
              Categories
            </h3>
            <div className="mt-2 flex flex-wrap gap-2">
              {org.categories.map((cat) => (
                <span
                  key={cat.id}
                  className="rounded-full bg-gray-100 px-3 py-1 text-xs font-medium text-gray-700"
                >
                  {cat.name}
                </span>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Action buttons */}
      {org.verificationStatus === 'pending' && (
        <div className="mt-6 flex gap-4">
          <button
            onClick={handleVerify}
            disabled={verifyMutation.isPending}
            className="rounded-lg bg-teal px-6 py-2.5 text-sm font-medium text-white transition-colors hover:bg-teal/90 disabled:opacity-50"
          >
            {verifyMutation.isPending ? 'Processing...' : 'Verify Organization'}
          </button>
          <button
            onClick={() => setShowRejectModal(true)}
            disabled={verifyMutation.isPending}
            className="rounded-lg bg-red-600 px-6 py-2.5 text-sm font-medium text-white transition-colors hover:bg-red-700 disabled:opacity-50"
          >
            Reject
          </button>
        </div>
      )}

      {/* Mutation error */}
      {verifyMutation.isError && (
        <div className="mt-4 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
          Failed to update organization status. Please try again.
        </div>
      )}

      {/* Reject modal */}
      {showRejectModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="mx-4 w-full max-w-md rounded-lg bg-white p-6 shadow-xl">
            <h2 className="font-display text-lg font-semibold text-navy">Reject Organization</h2>
            <p className="mt-1 text-sm text-gray-500">
              Please provide a reason for rejecting {org.name}. This will be sent to the
              organization.
            </p>
            <textarea
              value={rejectReason}
              onChange={(e) => {
                setRejectReason(e.target.value);
                if (reasonError) setReasonError('');
              }}
              placeholder="Enter rejection reason..."
              rows={4}
              className="mt-4 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder:text-gray-400 focus:border-coral focus:outline-none focus:ring-1 focus:ring-coral"
            />
            {reasonError && <p className="mt-1 text-xs text-red-600">{reasonError}</p>}
            <div className="mt-4 flex justify-end gap-3">
              <button
                onClick={() => {
                  setShowRejectModal(false);
                  setRejectReason('');
                  setReasonError('');
                }}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={handleReject}
                disabled={verifyMutation.isPending}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
              >
                {verifyMutation.isPending ? 'Rejecting...' : 'Reject'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
