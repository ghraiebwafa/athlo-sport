export function formatHms(totalSeconds: number): string {
  const s = Math.max(0, Math.floor(totalSeconds));
  const h = Math.floor(s / 3600);
  const m = Math.floor((s % 3600) / 60);
  const sec = s % 60;
  return [h, m, sec].map((n) => n.toString().padStart(2, '0')).join(':');
}

export function formatMmSs(totalSeconds: number): string {
  const s = Math.max(0, Math.floor(totalSeconds));
  const m = Math.floor(s / 60);
  const sec = s % 60;
  return `${m}:${sec.toString().padStart(2, '0')}`;
}

export function elapsedSeconds(startedAt: string, now: number): number {
  return Math.max(0, Math.floor((now - new Date(startedAt).getTime()) / 1000));
}

/** Rough estimate when no wearable data is available. */
export function estimateCaloriesBurned(elapsedSec: number, targetCalories: number, durationMinutes: number): number {
  if (durationMinutes <= 0) return Math.floor(elapsedSec / 60) * 8;
  const ratio = Math.min(1, elapsedSec / (durationMinutes * 60));
  return Math.max(1, Math.floor(targetCalories * ratio));
}

export function estimateHeartRate(elapsedSec: number): number {
  const base = 118;
  const peak = 155;
  const ramp = Math.min(1, elapsedSec / 600);
  return Math.floor(base + (peak - base) * ramp + (Math.sin(elapsedSec / 30) * 4));
}

export function estimateIntensity(progressPercent: number): number {
  return Math.min(95, Math.max(55, Math.floor(55 + progressPercent * 0.4)));
}

export function exerciseProgress(
  elapsedSec: number,
  durationMinutes: number,
  exerciseCount: number
): { index: number; progressPercent: number } {
  if (exerciseCount <= 0) return { index: 0, progressPercent: 0 };
  const total = Math.max(durationMinutes * 60, exerciseCount * 60);
  const ratio = Math.min(0.999, elapsedSec / total);
  const index = Math.min(exerciseCount - 1, Math.floor(ratio * exerciseCount));
  const slice = 1 / exerciseCount;
  const within = (ratio - index * slice) / slice;
  return { index, progressPercent: Math.floor(within * 100) };
}
