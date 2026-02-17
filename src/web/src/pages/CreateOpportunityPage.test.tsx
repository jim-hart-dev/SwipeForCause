import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import CreateOpportunityPage from './CreateOpportunityPage';

// Mock the hook
const mockMutateAsync = vi.fn();
vi.mock('../hooks/useCreateOpportunity', () => ({
  useCreateOpportunity: () => ({
    mutateAsync: mockMutateAsync,
    isPending: false,
    isError: false,
    error: null,
  }),
}));

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

function renderPage() {
  return render(
    <MemoryRouter>
      <CreateOpportunityPage />
    </MemoryRouter>,
  );
}

describe('CreateOpportunityPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockMutateAsync.mockResolvedValue({ opportunityId: 'abc-123', status: 'active' });
  });

  it('renders all section headers', () => {
    renderPage();
    expect(screen.getByText('Details')).toBeInTheDocument();
    expect(screen.getByText('Location')).toBeInTheDocument();
    expect(screen.getByText('Schedule')).toBeInTheDocument();
    expect(screen.getByText('Requirements')).toBeInTheDocument();
  });

  it('renders title and description fields', () => {
    renderPage();
    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
  });

  it('shows character counters for title and description', async () => {
    const user = userEvent.setup();
    renderPage();

    const titleInput = screen.getByLabelText(/title/i);
    await user.type(titleInput, 'Hello');
    expect(screen.getByText('5 / 200')).toBeInTheDocument();

    const descInput = screen.getByLabelText(/description/i);
    await user.type(descInput, 'World');
    expect(screen.getByText('5 / 5000')).toBeInTheDocument();
  });

  it('hides location field when remote toggle is on', async () => {
    const user = userEvent.setup();
    renderPage();

    // Location field should be visible by default
    expect(screen.getByLabelText(/address/i)).toBeInTheDocument();

    // Toggle remote
    const remoteToggle = screen.getByRole('switch', { name: /remote/i });
    await user.click(remoteToggle);

    // Location field should be hidden
    expect(screen.queryByLabelText(/address/i)).not.toBeInTheDocument();
  });

  it('shows date fields only for one_time schedule type', async () => {
    const user = userEvent.setup();
    renderPage();

    // Default is flexible — no date fields
    expect(screen.queryByLabelText(/start date/i)).not.toBeInTheDocument();

    // Select one_time
    const oneTimeRadio = screen.getByLabelText(/one.time/i);
    await user.click(oneTimeRadio);

    expect(screen.getByLabelText(/start date/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/end date/i)).toBeInTheDocument();
  });

  it('shows recurrence field only for recurring schedule type', async () => {
    const user = userEvent.setup();
    renderPage();

    expect(screen.queryByLabelText(/recurrence/i)).not.toBeInTheDocument();

    const recurringRadio = screen.getByLabelText(/recurring/i);
    await user.click(recurringRadio);

    expect(screen.getByLabelText(/recurrence/i)).toBeInTheDocument();
  });

  it('submits form with correct data for flexible schedule', async () => {
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/title/i), 'Beach Cleanup');
    await user.type(screen.getByLabelText(/description/i), 'Help us clean the beach');
    await user.type(screen.getByLabelText(/time commitment/i), '2-3 hours');

    const publishBtn = screen.getByRole('button', { name: /publish/i });
    await user.click(publishBtn);

    expect(mockMutateAsync).toHaveBeenCalledWith(
      expect.objectContaining({
        title: 'Beach Cleanup',
        description: 'Help us clean the beach',
        scheduleType: 'flexible',
        isRemote: false,
        timeCommitment: '2-3 hours',
      }),
    );
  });

  it('navigates to dashboard after successful submission', async () => {
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/title/i), 'Beach Cleanup');
    await user.type(screen.getByLabelText(/description/i), 'Help us clean the beach');

    const publishBtn = screen.getByRole('button', { name: /publish/i });
    await user.click(publishBtn);

    expect(mockNavigate).toHaveBeenCalledWith('/org/dashboard', expect.objectContaining({ replace: true }));
  });

  it('disables publish button when title or description is empty', () => {
    renderPage();
    const publishBtn = screen.getByRole('button', { name: /publish/i });
    expect(publishBtn).toBeDisabled();
  });
});
