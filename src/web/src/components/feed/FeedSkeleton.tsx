export default function FeedSkeleton() {
  return (
    <div className="h-[calc(100vh-48px)] w-full bg-navy/10 animate-pulse flex flex-col justify-end p-4 pb-16">
      {/* Org info skeleton */}
      <div className="flex items-center gap-2 mb-3">
        <div className="w-9 h-9 rounded-full bg-navy/20" />
        <div className="h-4 w-28 rounded bg-navy/20" />
      </div>
      {/* Title skeleton */}
      <div className="h-5 w-3/4 rounded bg-navy/20 mb-2" />
      <div className="h-4 w-1/2 rounded bg-navy/20 mb-4" />
      {/* CTA skeleton */}
      <div className="h-12 w-full rounded-xl bg-navy/20" />
    </div>
  );
}
