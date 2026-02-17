import { Link } from 'react-router-dom';
import type { FeedItem } from '../../types';

interface FeedItemOverlayProps {
  item: FeedItem;
}

export default function FeedItemOverlay({ item }: FeedItemOverlayProps) {
  const { organization, title, description } = item;

  return (
    <div className="absolute inset-0 flex flex-col justify-end pointer-events-none">
      {/* Gradient overlay */}
      <div
        className="absolute inset-0"
        style={{
          background: 'linear-gradient(transparent 60%, rgba(0,0,0,0.6) 100%)',
        }}
      />

      {/* Content */}
      <div className="relative z-10 p-4 pb-16 flex flex-col gap-3">
        {/* Org info */}
        <Link
          to={`/org/${organization.id}`}
          className="flex items-center gap-2 pointer-events-auto"
        >
          {organization.logoUrl ? (
            <img
              src={organization.logoUrl}
              alt={`${organization.name} logo`}
              className="w-9 h-9 rounded-full border-2 border-white object-cover"
            />
          ) : (
            <div className="w-9 h-9 rounded-full border-2 border-white bg-navy flex items-center justify-center text-white text-sm font-bold">
              {organization.name.charAt(0)}
            </div>
          )}
          <span className="text-white text-sm font-body font-medium">{organization.name}</span>
          {organization.isVerified && (
            <span aria-label="Verified organization" className="text-teal text-sm">
              ✓
            </span>
          )}
        </Link>

        {/* Title and description */}
        <div>
          <h2 className="text-white font-body font-bold text-lg leading-tight line-clamp-2">
            {title}
          </h2>
          {description && (
            <p className="text-white/80 font-body text-sm mt-1 line-clamp-2">{description}</p>
          )}
        </div>

        {/* CTA */}
        <button
          type="button"
          className="w-full bg-coral text-white font-body font-semibold text-base rounded-xl h-12 pointer-events-auto"
        >
          Volunteer Now
        </button>
      </div>
    </div>
  );
}
