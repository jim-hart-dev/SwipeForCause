import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, afterEach } from 'vitest';
import RelativeTime, { formatRelativeTime } from './RelativeTime';

describe('formatRelativeTime', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('returns "just now" for timestamps less than 60 seconds ago', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2025-01-15T12:00:30Z'));
    expect(formatRelativeTime('2025-01-15T12:00:00Z')).toBe('just now');
  });

  it('returns minutes for timestamps less than 60 minutes ago', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2025-01-15T12:25:00Z'));
    expect(formatRelativeTime('2025-01-15T12:00:00Z')).toBe('25m ago');
  });

  it('returns hours for timestamps less than 24 hours ago', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2025-01-15T15:00:00Z'));
    expect(formatRelativeTime('2025-01-15T12:00:00Z')).toBe('3h ago');
  });

  it('returns days for timestamps less than 7 days ago', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2025-01-18T12:00:00Z'));
    expect(formatRelativeTime('2025-01-15T12:00:00Z')).toBe('3d ago');
  });

  it('returns weeks for timestamps less than 4 weeks ago', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2025-01-29T12:00:00Z'));
    expect(formatRelativeTime('2025-01-15T12:00:00Z')).toBe('2w ago');
  });

  it('returns formatted date for timestamps older than 4 weeks', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2025-03-15T12:00:00Z'));
    const result = formatRelativeTime('2025-01-15T12:00:00Z');
    // toLocaleDateString output varies by locale, just verify it's not a relative format
    expect(result).not.toContain('ago');
    expect(result).not.toBe('just now');
  });
});

describe('RelativeTime component', () => {
  it('renders a <time> element with correct datetime attribute', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2025-01-15T12:05:00Z'));
    render(<RelativeTime date="2025-01-15T12:00:00Z" />);
    const timeEl = screen.getByText('5m ago');
    expect(timeEl.tagName).toBe('TIME');
    expect(timeEl).toHaveAttribute('datetime', '2025-01-15T12:00:00Z');
    vi.useRealTimers();
  });
});
