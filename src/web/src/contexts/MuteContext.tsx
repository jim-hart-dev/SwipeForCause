import { createContext, useCallback, useContext, useState, type ReactNode } from 'react';

interface MuteContextValue {
  isMuted: boolean;
  toggleMute: () => void;
  setMuted: (muted: boolean) => void;
}

const MuteContext = createContext<MuteContextValue | null>(null);

export function MuteProvider({ children }: { children: ReactNode }) {
  const [isMuted, setIsMuted] = useState(false);
  const toggleMute = useCallback(() => setIsMuted((prev) => !prev), []);
  const setMuted = useCallback((muted: boolean) => setIsMuted(muted), []);
  return (
    <MuteContext.Provider value={{ isMuted, toggleMute, setMuted }}>
      {children}
    </MuteContext.Provider>
  );
}

export function useMute(): MuteContextValue {
  const ctx = useContext(MuteContext);
  if (!ctx) throw new Error('useMute must be used within MuteProvider');
  return ctx;
}
