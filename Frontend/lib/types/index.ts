export type FitnessGoal = 'LoseWeight' | 'BuildMuscle' | 'StayActive';
export type UserRole = 'User' | 'Admin' | 'SuperAdmin';
export type WorkoutDifficulty = 'Beginner' | 'Intermediate' | 'Advanced';
export type WorkoutSessionStatus = 'InProgress' | 'Completed' | 'Cancelled';

export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  currentWeight: number;
  goalWeight: number;
  fitnessGoal: FitnessGoal;
  role: UserRole;
  goalProgressPercent: number;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserProfile;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  icon: string;
  programCount: number;
}

export interface Exercise {
  id: string;
  name: string;
  imageUrl?: string;
}

export interface ProgramListItem {
  id: string;
  name: string;
  description: string;
  durationMinutes: number;
  difficulty: WorkoutDifficulty;
  estimatedCalories: number;
  imageUrl?: string;
  isFeatured: boolean;
  categoryName: string;
  exerciseCount: number;
}

export interface ProgramExercise {
  id: string;
  exerciseId: string;
  name: string;
  orderIndex: number;
  sets: number;
  reps: number;
  durationSeconds?: number;
  imageUrl?: string;
}

export interface ProgramDetail extends Omit<ProgramListItem, 'exerciseCount' | 'categoryName'> {
  categoryId: string;
  categoryName: string;
  exercises: ProgramExercise[];
}

export interface ProgramExerciseInput {
  exerciseId: string;
  orderIndex: number;
  sets: number;
  reps: number;
  durationSeconds?: number;
}

export interface CreateProgramPayload {
  name: string;
  description: string;
  durationMinutes: number;
  difficulty: WorkoutDifficulty;
  estimatedCalories: number;
  imageUrl?: string;
  isFeatured: boolean;
  categoryId: string;
  exercises: ProgramExerciseInput[];
}

export interface AdminUserListItem {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  createdAt: string;
}

export interface AdminDashboardStats {
  totalUsers: number;
  totalAdmins: number;
  totalPrograms: number;
  totalExercises: number;
  completedWorkoutsToday: number;
  activeWorkoutsNow: number;
}

export interface WorkoutSetLog {
  id: string;
  programExerciseId: string;
  exerciseId: string;
  exerciseName: string;
  setNumber: number;
  repsCompleted: number;
  weightKg?: number;
  completed: boolean;
  loggedAt: string;
}

export interface WorkoutSession {
  id: string;
  programId: string;
  programName: string;
  startedAt: string;
  completedAt?: string;
  caloriesBurned?: number;
  status: WorkoutSessionStatus;
  durationMinutes?: number;
  sets?: WorkoutSetLog[];
}

export interface LiftPersonalRecord {
  exerciseId: string;
  exerciseName: string;
  weightKg: number;
  reps: number;
  achievedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ProgressData {
  totalWorkouts: number;
  totalCaloriesBurned: number;
  currentStreak: number;
  personalBests: number;
  goalProgressPercent: number;
  currentWeight: number;
  goalWeight: number;
  weeklyFrequency: { weekStart: string; workoutCount: number }[];
  recentWorkouts: {
    sessionId: string;
    programName: string;
    completedAt: string;
    caloriesBurned: number;
    durationMinutes: number;
  }[];
  personalRecords?: LiftPersonalRecord[];
}

export interface ApiErrorDetail {
  field: string;
  message: string;
}

export interface ApiErrorResponse {
  api: {
    error: {
      code: string;
      message: string;
      details?: ApiErrorDetail[];
    };
  };
}
