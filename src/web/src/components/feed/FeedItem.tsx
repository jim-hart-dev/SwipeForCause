import { forwardRef } from 'react';
import { motion } from 'framer-motion';
import FeedItemOverlay from './FeedItemOverlay';
import VideoPlayer from './VideoPlayer';
import type { FeedItem as FeedItemType } from '../../types';

interface FeedItemProps {
  item: FeedItemType;
  isActive: boolean;
}

const FeedItem = forwardRef<HTMLDivElement, FeedItemProps>(({ item, isActive }, ref) => {
  const media = item.media[0];
  const isVideo = item.mediaType === 'video' && media;

  return (
    <motion.div
      ref={ref}
      className="relative h-[calc(100vh-48px)] w-full snap-start overflow-hidden bg-navy"
      data-index
      animate={{ scale: isActive ? 1 : 0.98 }}
      transition={{ duration: 0.2 }}
    >
      {isVideo ? (
        <VideoPlayer
          videoUrl={media.url}
          thumbnailUrl={media.thumbnailUrl}
          isActive={isActive}
        />
      ) : (
        <>
          {media && (
            <img
              src={media.url}
              alt={item.title}
              className="absolute inset-0 w-full h-full object-cover"
            />
          )}
        </>
      )}
      <FeedItemOverlay item={item} />
    </motion.div>
  );
});

FeedItem.displayName = 'FeedItem';

export default FeedItem;
