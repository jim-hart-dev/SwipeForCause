import { useRef, useEffect, useState, useCallback } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import { useMute } from '../../contexts/MuteContext';

interface VideoPlayerProps {
  videoUrl: string;
  thumbnailUrl: string | null;
  isActive: boolean;
}

export default function VideoPlayer({ videoUrl, thumbnailUrl, isActive }: VideoPlayerProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const { isMuted, toggleMute, setMuted } = useMute();
  const [progress, setProgress] = useState(0);
  const [hasError, setHasError] = useState(false);
  const [showIndicator, setShowIndicator] = useState(false);
  const [retryKey, setRetryKey] = useState(0);

  const [prefersReducedMotion, setPrefersReducedMotion] = useState(
    () =>
      typeof window !== 'undefined' &&
      typeof window.matchMedia === 'function' &&
      window.matchMedia('(prefers-reduced-motion: reduce)').matches,
  );

  useEffect(() => {
    if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') return;
    const mq = window.matchMedia('(prefers-reduced-motion: reduce)');
    const handler = (e: MediaQueryListEvent) => setPrefersReducedMotion(e.matches);
    mq.addEventListener('change', handler);
    return () => mq.removeEventListener('change', handler);
  }, []);

  // Play/pause based on isActive
  useEffect(() => {
    const video = videoRef.current;
    if (!video || prefersReducedMotion) return;

    if (isActive && !hasError) {
      video.currentTime = 0;
      video.play().catch(() => {
        if (!video.muted) {
          // Browser blocked unmuted autoplay — fall back to muted
          setMuted(true);
          video.muted = true;
          video.play().catch(() => setHasError(true));
        } else {
          setHasError(true);
        }
      });
    } else {
      video.pause();
    }
  }, [isActive, hasError, prefersReducedMotion, retryKey, setMuted]);

  // Track progress
  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;

    const onTimeUpdate = () => {
      if (video.duration) {
        setProgress((video.currentTime / video.duration) * 100);
      }
    };

    video.addEventListener('timeupdate', onTimeUpdate);
    return () => video.removeEventListener('timeupdate', onTimeUpdate);
  }, [retryKey]);

  // Handle error
  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;

    const onError = () => setHasError(true);
    video.addEventListener('error', onError);
    return () => video.removeEventListener('error', onError);
  }, [retryKey]);

  const handleTap = useCallback(() => {
    toggleMute();
    setShowIndicator(true);
  }, [toggleMute]);

  useEffect(() => {
    if (!showIndicator) return;
    const id = setTimeout(() => setShowIndicator(false), 1000);
    return () => clearTimeout(id);
  }, [showIndicator]);

  const handleRetry = useCallback(() => {
    setHasError(false);
    setRetryKey((k) => k + 1);
  }, []);

  // Reduced motion: show static thumbnail
  if (prefersReducedMotion) {
    return (
      <div className="absolute inset-0" data-testid="reduced-motion-thumbnail">
        {thumbnailUrl && (
          <img src={thumbnailUrl} alt="" className="h-full w-full object-cover" />
        )}
      </div>
    );
  }

  return (
    <div className="absolute inset-0">
      <video
        key={retryKey}
        ref={videoRef}
        data-testid="video-player"
        src={videoUrl}
        muted={isMuted}
        loop
        playsInline
        preload="metadata"
        poster={thumbnailUrl ?? undefined}
        className="h-full w-full object-cover"
      />

      {/* Tap area for mute toggle */}
      <button
        type="button"
        data-testid="video-tap-area"
        className="absolute inset-0 z-10 appearance-none bg-transparent border-none cursor-default p-0"
        aria-label={isMuted ? 'Unmute video' : 'Mute video'}
        onClick={handleTap}
      />

      {/* Mute indicator */}
      <AnimatePresence>
        {showIndicator && (
          <motion.div
            data-testid="mute-indicator"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
            className="pointer-events-none absolute inset-0 z-20 flex items-center justify-center"
          >
            <div className="flex h-16 w-16 items-center justify-center rounded-full bg-white/80">
              <span className="text-2xl" aria-hidden="true">
                {isMuted ? '\u{1F507}' : '\u{1F50A}'}
              </span>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Progress bar */}
      <div
        data-testid="video-progress"
        className="absolute bottom-0 left-0 right-0 z-30 h-0.5"
      >
        <div
          className="h-full bg-coral transition-[width] duration-200"
          style={{ width: `${progress}%` }}
        />
      </div>

      {/* Error state */}
      {hasError && (
        <div
          data-testid="video-error"
          className="absolute inset-0 z-30 flex items-center justify-center"
        >
          {thumbnailUrl && (
            <img
              src={thumbnailUrl}
              alt=""
              className="absolute inset-0 h-full w-full object-cover blur-sm"
            />
          )}
          <button
            type="button"
            data-testid="video-retry"
            onClick={handleRetry}
            className="relative z-10 flex h-16 w-16 items-center justify-center rounded-full bg-white/80"
            aria-label="Retry video"
          >
            <svg viewBox="0 0 24 24" fill="currentColor" className="ml-1 h-8 w-8 text-navy">
              <path d="M8 5v14l11-7z" />
            </svg>
          </button>
        </div>
      )}
    </div>
  );
}
