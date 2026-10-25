import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { api, ApiError } from '../api';
import { ExplainPanel } from './ExplainPanel';

afterEach(() => vi.restoreAllMocks());

describe('ExplainPanel', () => {
  it('shows a loading state and then the explanation', async () => {
    let resolve!: (value: Awaited<ReturnType<typeof api.explainItem>>) => void;
    vi.spyOn(api, 'explainItem').mockReturnValue(new Promise((r) => { resolve = r; }));

    render(<ExplainPanel itemId="aw-007" aiEnabled />);
    await userEvent.click(screen.getByRole('button', { name: 'Explain with AI' }));

    expect(screen.getByRole('button', { name: /Explaining/ })).toBeDisabled();

    resolve({ explanation: 'Anyone can edit.', businessRisk: 'Data loss.', fixSteps: ['Remove the link'], generatedBy: 'Test' });

    expect(await screen.findByText('Anyone can edit.')).toBeInTheDocument();
    expect(screen.getByText('Remove the link')).toBeInTheDocument();
    expect(api.explainItem).toHaveBeenCalledWith('aw-007');
  });

  it('shows the error and lets the user retry', async () => {
    vi.spyOn(api, 'explainItem').mockRejectedValue(new ApiError('AI service unavailable', 502));

    render(<ExplainPanel itemId="aw-007" aiEnabled />);
    await userEvent.click(screen.getByRole('button', { name: 'Explain with AI' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('AI service unavailable');
    expect(screen.getByRole('button', { name: 'Try again' })).toBeInTheDocument();
  });
});
