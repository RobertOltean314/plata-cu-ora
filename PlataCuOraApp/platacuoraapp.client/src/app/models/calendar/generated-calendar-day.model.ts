import { TeachingActivity } from './teaching-activity.model';

export interface GeneratedCalendarDay {
  date: Date;
  dayOfWeek: string;
  isWorkingDay: boolean;
  weekNumber: string | null;
  parity: 'Impar' | 'Par' | null;
  activities: TeachingActivity[];
}