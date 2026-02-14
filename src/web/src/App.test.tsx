import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';

// Mock the routes module since it may not exist yet
vi.mock('./routes', () => ({
  router: null,
}));

// Mock RouterProvider to avoid needing a real router
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    RouterProvider: () => <div data-testid="router-outlet">App Loaded</div>,
  };
});

import App from './App';

describe('App', () => {
  it('renders without crashing', () => {
    render(<App />);
    expect(screen.getByTestId('router-outlet')).toBeInTheDocument();
  });
});
