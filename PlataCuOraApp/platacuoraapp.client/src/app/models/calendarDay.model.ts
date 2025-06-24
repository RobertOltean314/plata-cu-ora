// export interface CalendarDay {
//     date: Date;
//     dayOfWeek: string;
//     isWorkDay: boolean;
//     parityType: 'Par' | 'Impar' | null;
//     parity: string; 
//     isHoliday: boolean;
//     holidayName?: string;
//     weekNumber?: number;
//     workDayStatus?: '' | 'Zi lucratoare' | 'Zi nelucratoare';
// }

export interface CalendarDay {
  date: string;           
  dayOfWeek: string;      
  isWorkingDay: boolean;  
  parity: string;         
  workDayStatus?: string; 
}



