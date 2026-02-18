import { useEffect, useCallback } from 'react';
import { useFeed } from '../../hooks/useFeed';
import { useActiveIndex } from '../../hooks/useActiveIndex';
import FeedItem from './FeedItem';
import FeedSkeleton from './FeedSkeleton';

const VIRTUALIZATION_BUFFER = 2;

export default function FeedContainer() {
  const { items, isLoading, isSuccess, isError, hasNextPage, fetchNextPage, isFetchingNextPage } =
    useFeed();
  const { activeIndex, setItemRef } = useActiveIndex();

  // Prefetch next page when near bottom
  useEffect(() => {
    if (items.length > 0 && activeIndex >= items.length - 2 && hasNextPage && !isFetchingNextPage) {
      fetchNextPage();
    }
  }, [activeIndex, items.length, hasNextPage, isFetchingNextPage, fetchNextPage]);

  // Preload next video
  useEffect(() => {
    const nextItem = items[activeIndex + 1];
    if (!nextItem || nextItem.mediaType !== 'video' || !nextItem.media[0]) return;

    const url = nextItem.media[0].url;
    const existing = document.querySelector(`link[rel="preload"][href="${url}"]`);
    if (existing) return;

    const link = document.createElement('link');
    link.rel = 'preload';
    link.as = 'video';
    link.href = url;
    document.head.appendChild(link);

    return () => {
      link.remove();
    };
  }, [activeIndex, items]);

  const refCallback = useCallback(
    (index: number) => (el: HTMLDivElement | null) => {
      setItemRef(index, el);
    },
    [setItemRef],
  );

  if (isLoading) {
    return (
      <div data-testid="feed-skeleton" className="h-[calc(100vh-48px)] overflow-hidden">
        <FeedSkeleton />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="h-[calc(100vh-48px)] flex flex-col items-center justify-center bg-cream px-4">
        <p className="text-navy font-body text-lg mb-4">Something went wrong</p>
        <button
          type="button"
          onClick={() => window.location.reload()}
          className="bg-coral text-white font-body font-semibold px-6 py-3 rounded-xl"
        >
          Try Again
        </button>
      </div>
    );
  }

  if (isSuccess && items.length === 0) {
    return (
      <div className="h-[calc(100vh-48px)] flex flex-col items-center justify-center bg-cream px-8 text-center">
        <p className="text-navy font-body text-lg">
          Follow organizations and select interests to personalize your feed
        </p>
      </div>
    );
  }

  return (
    <div className="h-[calc(100vh-48px)] overflow-y-scroll snap-y snap-mandatory">
      {items.map((item, index) => {
        const isInWindow =
          index >= activeIndex - VIRTUALIZATION_BUFFER &&
          index <= activeIndex + VIRTUALIZATION_BUFFER;

        if (!isInWindow) {
          return (
            <div
              key={item.postId}
              className="h-[calc(100vh-48px)] snap-start"
              data-index={index}
              ref={refCallback(index)}
            />
          );
        }

        return (
          <FeedItem
            key={item.postId}
            ref={refCallback(index)}
            item={item}
            index={index}
            isActive={index === activeIndex}
          />
        );
      })}
    </div>
  );
}
