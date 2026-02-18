import { forwardRef } from 'react';
import { motion } from 'framer-motion';
import FeedItemOverlay from './FeedItemOverlay';
import type { FeedItem as FeedItemType } from '../../types';

interface FeedItemProps {
  item: FeedItemType;
  isActive: boolean;
}

function getImageUrl(item: FeedItemType): string {
  const media = item.media[0];
  if (!media) return '';
  if (item.mediaType === 'video' && media.thumbnailUrl) return media.thumbnailUrl;
  return media.url;
}

const FeedItem = forwardRef<HTMLDivElement, FeedItemProps>(({ item, isActive }, ref) => {
  const imageUrl = getImageUrl(item);

  return (
    <motion.div
      ref={ref}
      className="relative h-[calc(100vh-48px)] w-full snap-start overflow-hidden bg-navy"
      data-index
      animate={{ scale: isActive ? 1 : 0.98 }}
      transition={{ duration: 0.2 }}
    >
      {imageUrl && (
        <img
          src={imageUrl}
          alt={item.title}
          className="absolute inset-0 w-full h-full object-cover"
        />
      )}
      <FeedItemOverlay item={item} />
    </motion.div>
  );
});

FeedItem.displayName = 'FeedItem';

export default FeedItem;
