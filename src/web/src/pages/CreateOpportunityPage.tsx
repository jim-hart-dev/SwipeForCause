import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateOpportunity } from '../hooks/useCreateOpportunity';
import type { CreateOpportunityRequest } from '../types';

type ScheduleType = 'one_time' | 'recurring' | 'flexible';

export default function CreateOpportunityPage() {
  const navigate = useNavigate();
  const createMutation = useCreateOpportunity();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [locationAddress, setLocationAddress] = useState('');
  const [isRemote, setIsRemote] = useState(false);
  const [scheduleType, setScheduleType] = useState<ScheduleType>('flexible');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [recurrenceDesc, setRecurrenceDesc] = useState('');
  const [timeCommitment, setTimeCommitment] = useState('');
  const [volunteersNeeded, setVolunteersNeeded] = useState('');
  const [skillsRequired, setSkillsRequired] = useState('');
  const [minimumAge, setMinimumAge] = useState('');

  const isValid = title.trim() !== '' && description.trim() !== '';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!isValid) return;

    const data: CreateOpportunityRequest = {
      title: title.trim(),
      description: description.trim(),
      isRemote,
      scheduleType,
    };

    if (!isRemote && locationAddress.trim()) {
      data.locationAddress = locationAddress.trim();
    }

    if (scheduleType === 'one_time') {
      if (startDate) data.startDate = new Date(startDate).toISOString();
      if (endDate) data.endDate = new Date(endDate).toISOString();
    }

    if (scheduleType === 'recurring' && recurrenceDesc.trim()) {
      data.recurrenceDesc = recurrenceDesc.trim();
    }

    if (timeCommitment.trim()) data.timeCommitment = timeCommitment.trim();
    if (volunteersNeeded) data.volunteersNeeded = parseInt(volunteersNeeded, 10);
    if (skillsRequired.trim()) data.skillsRequired = skillsRequired.trim();
    if (minimumAge) data.minimumAge = parseInt(minimumAge, 10);

    await createMutation.mutateAsync(data);
    navigate('/org/dashboard', { replace: true });
  };

  const inputClass =
    'w-full rounded-lg border border-navy/20 bg-white px-4 py-3.5 font-body text-navy placeholder:text-navy/40 focus:border-coral focus:outline-none focus:ring-1 focus:ring-coral';
  const labelClass = 'block text-sm font-body font-medium text-navy mb-1';
  const sectionClass = 'space-y-4';

  return (
    <div className="min-h-screen bg-cream px-4 pb-8 pt-6">
      <div className="mx-auto w-full max-w-lg">
        <h1 className="mb-6 font-display text-2xl text-navy">Create Opportunity</h1>

        <form onSubmit={handleSubmit} className="space-y-8">
          {/* Details Section */}
          <section className={sectionClass}>
            <h2 className="font-display text-lg text-navy">Details</h2>

            <div>
              <label htmlFor="title" className={labelClass}>
                Title
              </label>
              <input
                id="title"
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="e.g. Beach Cleanup Day"
                maxLength={200}
                className={inputClass}
              />
              <p className="mt-1 text-right text-xs font-body text-navy/40">
                {title.length} / 200
              </p>
            </div>

            <div>
              <label htmlFor="description" className={labelClass}>
                Description
              </label>
              <textarea
                id="description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Describe the opportunity, what volunteers will do, and the impact they'll make..."
                maxLength={5000}
                rows={5}
                className={inputClass + ' resize-none'}
              />
              <p className="mt-1 text-right text-xs font-body text-navy/40">
                {description.length} / 5000
              </p>
            </div>
          </section>

          {/* Location Section */}
          <section className={sectionClass}>
            <h2 className="font-display text-lg text-navy">Location</h2>

            <div className="flex items-center justify-between">
              <span className="font-body text-sm text-navy">This is a remote opportunity</span>
              <button
                type="button"
                role="switch"
                aria-checked={isRemote}
                aria-label="Remote opportunity"
                onClick={() => setIsRemote(!isRemote)}
                className={`relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors ${
                  isRemote ? 'bg-teal' : 'bg-navy/20'
                }`}
              >
                <span
                  className={`pointer-events-none inline-block h-5 w-5 rounded-full bg-white shadow-sm transition-transform ${
                    isRemote ? 'translate-x-5' : 'translate-x-0'
                  }`}
                />
              </button>
            </div>

            {!isRemote && (
              <div>
                <label htmlFor="locationAddress" className={labelClass}>
                  Address
                </label>
                <input
                  id="locationAddress"
                  type="text"
                  value={locationAddress}
                  onChange={(e) => setLocationAddress(e.target.value)}
                  placeholder="123 Main St, City, State"
                  maxLength={500}
                  className={inputClass}
                />
              </div>
            )}
          </section>

          {/* Schedule Section */}
          <section className={sectionClass}>
            <h2 className="font-display text-lg text-navy">Schedule</h2>

            <fieldset>
              <legend className="sr-only">Schedule type</legend>
              <div className="flex gap-3">
                {(['flexible', 'one_time', 'recurring'] as const).map((type) => {
                  const labels: Record<ScheduleType, string> = {
                    flexible: 'Flexible',
                    one_time: 'One-time',
                    recurring: 'Recurring',
                  };
                  return (
                    <label
                      key={type}
                      className={`flex-1 cursor-pointer rounded-lg border-2 px-3 py-2.5 text-center text-sm font-body font-medium transition-all ${
                        scheduleType === type
                          ? 'border-coral bg-coral/10 text-coral'
                          : 'border-navy/10 text-navy/60 hover:border-navy/20'
                      }`}
                    >
                      <input
                        type="radio"
                        name="scheduleType"
                        value={type}
                        checked={scheduleType === type}
                        onChange={() => setScheduleType(type)}
                        className="sr-only"
                      />
                      {labels[type]}
                    </label>
                  );
                })}
              </div>
            </fieldset>

            {scheduleType === 'one_time' && (
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label htmlFor="startDate" className={labelClass}>
                    Start date
                  </label>
                  <input
                    id="startDate"
                    type="datetime-local"
                    value={startDate}
                    onChange={(e) => setStartDate(e.target.value)}
                    className={inputClass}
                  />
                </div>
                <div>
                  <label htmlFor="endDate" className={labelClass}>
                    End date
                  </label>
                  <input
                    id="endDate"
                    type="datetime-local"
                    value={endDate}
                    onChange={(e) => setEndDate(e.target.value)}
                    className={inputClass}
                  />
                </div>
              </div>
            )}

            {scheduleType === 'recurring' && (
              <div>
                <label htmlFor="recurrenceDesc" className={labelClass}>
                  Recurrence description
                </label>
                <input
                  id="recurrenceDesc"
                  type="text"
                  value={recurrenceDesc}
                  onChange={(e) => setRecurrenceDesc(e.target.value)}
                  placeholder="e.g. Every Saturday 9am-12pm"
                  maxLength={500}
                  className={inputClass}
                />
              </div>
            )}

            <div>
              <label htmlFor="timeCommitment" className={labelClass}>
                Time commitment
              </label>
              <input
                id="timeCommitment"
                type="text"
                value={timeCommitment}
                onChange={(e) => setTimeCommitment(e.target.value)}
                placeholder="e.g. 2-3 hours per week"
                maxLength={100}
                className={inputClass}
              />
            </div>
          </section>

          {/* Requirements Section */}
          <section className={sectionClass}>
            <h2 className="font-display text-lg text-navy">Requirements</h2>

            <div>
              <label htmlFor="volunteersNeeded" className={labelClass}>
                Volunteers needed
              </label>
              <input
                id="volunteersNeeded"
                type="number"
                value={volunteersNeeded}
                onChange={(e) => setVolunteersNeeded(e.target.value)}
                placeholder="Optional"
                min={1}
                className={inputClass}
              />
            </div>

            <div>
              <label htmlFor="skillsRequired" className={labelClass}>
                Skills / requirements
              </label>
              <input
                id="skillsRequired"
                type="text"
                value={skillsRequired}
                onChange={(e) => setSkillsRequired(e.target.value)}
                placeholder="e.g. Must be comfortable outdoors"
                maxLength={500}
                className={inputClass}
              />
            </div>

            <div>
              <label htmlFor="minimumAge" className={labelClass}>
                Minimum age
              </label>
              <input
                id="minimumAge"
                type="number"
                value={minimumAge}
                onChange={(e) => setMinimumAge(e.target.value)}
                placeholder="Optional"
                min={1}
                max={120}
                className={inputClass}
              />
            </div>
          </section>

          {/* Submit */}
          <button
            type="submit"
            disabled={!isValid || createMutation.isPending}
            className="w-full rounded-lg bg-coral py-3.5 font-body font-semibold text-white transition-colors hover:bg-coral/90 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {createMutation.isPending ? 'Publishing...' : 'Publish Opportunity'}
          </button>

          {createMutation.isError && (
            <p className="text-center text-sm font-body text-red-500">
              {createMutation.error.message}
            </p>
          )}
        </form>
      </div>
    </div>
  );
}
