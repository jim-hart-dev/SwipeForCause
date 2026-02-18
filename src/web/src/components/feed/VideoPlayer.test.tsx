import { render, screen, act, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import VideoPlayer from './VideoPlayer';
import { MuteProvider } from '../../contexts/MuteContext';

// Mock play/pause on HTMLVideoElement for jsdom
let playMock: ReturnType<typeof vi.fn>;
let pauseMock: ReturnType<typeof vi.fn>;

beforeEach(() => {
  playMock = vi.fn().mockResolvedValue(undefined);
  pauseMock = vi.fn();
  Object.defineProperty(HTMLVideoElement.prototype, 'play', {
    configurable: true,
    value: playMock,
  });
  Object.defineProperty(HTMLVideoElement.prototype, 'pause', {
    configurable: true,
    value: pauseMock,
  });
});

const defaultProps = {
  videoUrl: 'https://cdn.example.com/video.mp4',
  thumbnailUrl: 'https://cdn.example.com/thumb.jpg',
  isActive: false,
};

function renderPlayer(props = {}) {
  return render(
    <MuteProvider>
      <VideoPlayer {...defaultProps} {...props} />
    </MuteProvider>,
  );
}

describe('VideoPlayer', () => {
  it('renders a video element with correct attributes', () => {
    renderPlayer();
    const video = screen.getByTestId('video-player') as HTMLVideoElement;
    expect(video.tagName).toBe('VIDEO');
    expect(video).toHaveAttribute('playsinline');
    expect(video.loop).toBe(true);
    expect(video.muted).toBe(false);
  });

  it('calls play when isActive becomes true', async () => {
    const { rerender } = render(
      <MuteProvider>
        <VideoPlayer {...defaultProps} isActive={false} />
      </MuteProvider>,
    );
    expect(playMock).not.toHaveBeenCalled();

    rerender(
      <MuteProvider>
        <VideoPlayer {...defaultProps} isActive={true} />
      </MuteProvider>,
    );
    expect(playMock).toHaveBeenCalled();
  });

  it('calls pause when isActive becomes false', async () => {
    const { rerender } = render(
      <MuteProvider>
        <VideoPlayer {...defaultProps} isActive={true} />
      </MuteProvider>,
    );

    rerender(
      <MuteProvider>
        <VideoPlayer {...defaultProps} isActive={false} />
      </MuteProvider>,
    );
    expect(pauseMock).toHaveBeenCalled();
  });

  it('toggles mute on tap', async () => {
    const user = userEvent.setup();
    renderPlayer({ isActive: true });
    const video = screen.getByTestId('video-player') as HTMLVideoElement;
    expect(video.muted).toBe(false);

    await user.click(screen.getByTestId('video-tap-area'));
    expect(video.muted).toBe(true);
  });

  it('shows mute indicator briefly on tap', async () => {
    const user = userEvent.setup();
    renderPlayer({ isActive: true });

    await user.click(screen.getByTestId('video-tap-area'));
    expect(screen.getByTestId('mute-indicator')).toBeInTheDocument();

    await waitFor(
      () => {
        expect(screen.queryByTestId('mute-indicator')).not.toBeInTheDocument();
      },
      { timeout: 3000 },
    );
  });

  it('renders progress bar', () => {
    renderPlayer({ isActive: true });
    expect(screen.getByTestId('video-progress')).toBeInTheDocument();
  });

  it('shows error state with thumbnail on video error', () => {
    renderPlayer({ isActive: true });
    const video = screen.getByTestId('video-player') as HTMLVideoElement;
    act(() => {
      video.dispatchEvent(new Event('error'));
    });

    expect(screen.getByTestId('video-error')).toBeInTheDocument();
    const img = screen.getByRole('presentation') as HTMLImageElement;
    expect(img.tagName).toBe('IMG');
    expect(img).toHaveAttribute('src', defaultProps.thumbnailUrl);
  });

  it('retries video load when error play button is tapped', async () => {
    const user = userEvent.setup();
    renderPlayer({ isActive: true });
    const video = screen.getByTestId('video-player') as HTMLVideoElement;
    act(() => {
      video.dispatchEvent(new Event('error'));
    });

    await user.click(screen.getByTestId('video-retry'));
    expect(screen.queryByTestId('video-error')).not.toBeInTheDocument();
  });

  it('resets currentTime to 0 when becoming active again', () => {
    const { rerender } = render(
      <MuteProvider>
        <VideoPlayer {...defaultProps} isActive={true} />
      </MuteProvider>,
    );
    const video = screen.getByTestId('video-player') as HTMLVideoElement;
    // Simulate video having played partway
    Object.defineProperty(video, 'currentTime', { writable: true, value: 15 });

    // Deactivate then reactivate
    rerender(
      <MuteProvider>
        <VideoPlayer {...defaultProps} isActive={false} />
      </MuteProvider>,
    );
    rerender(
      <MuteProvider>
        <VideoPlayer {...defaultProps} isActive={true} />
      </MuteProvider>,
    );
    expect(video.currentTime).toBe(0);
  });

  it('shows thumbnail instead of autoplay when prefers-reduced-motion', () => {
    window.matchMedia = vi.fn().mockImplementation((query: string) => ({
      matches: query === '(prefers-reduced-motion: reduce)',
      media: query,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      addListener: vi.fn(),
      removeListener: vi.fn(),
      onchange: null,
      dispatchEvent: vi.fn(),
    }));

    renderPlayer({ isActive: true });
    expect(playMock).not.toHaveBeenCalled();
    expect(screen.getByTestId('reduced-motion-thumbnail')).toBeInTheDocument();
  });
});
