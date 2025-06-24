export interface CalendarDay {
    date: Date;
    dayOfWeek: string;
    isWorkDay: boolean;
    parityType: 'Par' | 'Impar' | null;
    isHoliday: boolean;
    holidayName?: string;
    weekNumber?: number;
  }