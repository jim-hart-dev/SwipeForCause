import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect } from 'vitest';
import { MuteProvider, useMute } from './MuteContext';

function TestConsumer() {
  const { isMuted, toggleMute } = useMute();
  return (
    <div>
      <span data-testid="muted">{String(isMuted)}</span>
      <button onClick={toggleMute}>toggle</button>
    </div>
  );
}

describe('MuteContext', () => {
  it('defaults to unmuted', () => {
    render(
      <MuteProvider>
        <TestConsumer />
      </MuteProvider>,
    );
    expect(screen.getByTestId('muted')).toHaveTextContent('false');
  });

  it('toggles mute state', async () => {
    const user = userEvent.setup();
    render(
      <MuteProvider>
        <TestConsumer />
      </MuteProvider>,
    );
    await user.click(screen.getByRole('button', { name: 'toggle' }));
    expect(screen.getByTestId('muted')).toHaveTextContent('true');
    await user.click(screen.getByRole('button', { name: 'toggle' }));
    expect(screen.getByTestId('muted')).toHaveTextContent('false');
  });

  it('throws when used outside provider', () => {
    const spy = vi.spyOn(console, 'error').mockImplementation(() => {});
    expect(() => render(<TestConsumer />)).toThrow('useMute must be used within MuteProvider');
    spy.mockRestore();
  });
});
