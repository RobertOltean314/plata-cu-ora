import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { HolidayService, PublicHoliday } from '../services/holiday-service/holiday.service';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { CalendarDay } from '../../models/calendarDay.model';

@Component({
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.css'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  providers: [DatePipe]
})
export class CalendarComponent implements OnInit {
  calendarData: CalendarDay[] = [];
  filteredCalendarData: CalendarDay[] = [];
  filterForm: FormGroup;
  holidays: Map<string, PublicHoliday> = new Map();
  loading = false;
  error: string | null = null;

  dayNames = {
    0: 'Duminica',
    1: 'Luni',
    2: 'Marti',
    3: 'Miercuri',
    4: 'Joi',
    5: 'Vineri',
    6: 'Sambata'
  };

  fallSemesterStartDate = new Date(2024, 8, 30); // September 30, 2024
  springSemesterStartDate = new Date(2025, 1, 24); // February 24, 2025

  constructor(
    private fb: FormBuilder,
    private holidayService: HolidayService
  ) {
    this.filterForm = this.fb.group({
      startDate: [new Date(2025, 4, 1)], // May 1, 2025
      endDate: [new Date(2025, 4, 31)]   // May 31, 2025
    });
  }

  ngOnInit(): void {
    this.loadHolidaysAndGenerateCalendar();
  }

  /**
  * Loads public holiday data for selected years and generates the calendar.
  * 
  * This method fetches public holidays from the holiday service for the years
  * specified in the date filter. It then processes this data to generate
  * a calendar with accurate holiday information, working days, and academic
  * week parity status.
  * 
  * The method exhibits the following behavior:
  * 1. Sets the component to a loading state
  * 2. Determines which years need holiday data based on the date filter
  * 3. Makes parallel API requests for each unique year
  * 4. Processes the responses and updates the internal holiday cache
  * 5. Generates the calendar data with holiday information
  * 6. Applies the date filter to show only the selected range
  * 
  * If any API requests fail for specific years, those years will have
  * no holiday data but the calendar will still be generated with the
  * available data. If the entire operation fails, the method falls back
  * to generating a calendar without any holiday data.
  * 
  * @returns {void} This method does not return a value but updates component state
  */
  loadHolidaysAndGenerateCalendar(): void {
    this.loading = true;
    this.error = null;
    
    const startYear = this.filterForm.get('startDate')?.value?.getFullYear() || new Date().getFullYear();
    const endYear = this.filterForm.get('endDate')?.value?.getFullYear() || startYear;
    
    const years = [...new Set([startYear, endYear])];
    
    forkJoin(
      years.map(year => 
        this.holidayService.getPublicHolidays(year).pipe(
          catchError(err => {
            console.error(`Error loading holidays for ${year}:`, err);
            return of([] as PublicHoliday[]);
          })
        )
      )
    ).pipe(
      finalize(() => this.loading = false)
    ).subscribe({
      next: (holidaysArrays) => {
        this.holidays.clear();
        holidaysArrays.flat().forEach(holiday => {
          const dateKey = this.formatDateKey(new Date(holiday.date));
          this.holidays.set(dateKey, holiday);
        });
        
        this.generateCalendarData();
        this.applyDateFilter();
      },
      error: (err) => {
        this.error = 'Failed to load holidays. Using default calendar.';
        console.error('Error loading holidays:', err);
        
        this.generateCalendarData();
        this.applyDateFilter();
      }
    });
  }

  generateCalendarData(): void {
    const startYear = this.filterForm.get('startDate')?.value?.getFullYear() || 2025;
    const endYear = this.filterForm.get('endDate')?.value?.getFullYear() || startYear;
    
    const startDate = new Date(startYear, 0, 1);  // Jan 1
    const endDate = new Date(endYear, 11, 31);    // Dec 31
    
    const academicCalendar = this.calculateAcademicWeeks(startDate, endDate);
    
    this.calendarData = this.generateDaysInRange(startDate, endDate, academicCalendar);
    this.filteredCalendarData = [...this.calendarData];
  }

  /**
  * Calculates academic week numbers for each date in the specified range.
  * 
  * This method creates a mapping between calendar dates and their corresponding
  * academic week numbers. It processes each academic year (both fall and spring
  * semesters) in the given date range. For each date, it determines:
  * - The academic week number (starting from 1 for each semester)
  * - Whether the week is odd or even (used for Par/Impar determination)
  * 
  * The academic calendar is structured as follows:
  * - Fall semester: Starts around September 30, ends around January 15
  * - Spring semester: Starts around February 24, ends around June 15
  * - Each semester starts with Week 1 (Odd/Impar)
  * - Consecutive weeks alternate between Odd and Even
  * 
  * This method handles multiple academic years if the date range spans across years.
  * 
  * @param {Date} startDate - The beginning date of the range to calculate
  * @param {Date} endDate - The ending date of the range to calculate
  * @returns {Map<string, { weekNumber: number, isOdd: boolean }>} A map where:
  *   - Keys are formatted date strings (YYYY-MM-DD format)
  *   - Values are objects containing:
  *     - weekNumber: The academic week number (1-based)
  *     - isOdd: Whether the week number is odd (true) or even (false)
  */
  calculateAcademicWeeks(startDate: Date, endDate: Date): Map<string, { weekNumber: number, isOdd: boolean }> {
    const calendar = new Map<string, { weekNumber: number, isOdd: boolean }>();
    const startYear = startDate.getFullYear();
    const endYear = endDate.getFullYear();
    
    // For each academic year in range
    for (let year = startYear; year <= endYear; year++) {
      // Fall semester (year)
      const fallStart = new Date(year, 8, 30); // Approximate - Sep 30 
      this.calculateSemesterWeeks(fallStart, new Date(year + 1, 1, 15), calendar, 1);
      
      // Spring semester (year)
      const springStart = new Date(year, 1, 24); // Approximate - Feb 24
      this.calculateSemesterWeeks(springStart, new Date(year, 6, 15), calendar, 1);
    }
    
    return calendar;
  }
  
  /**
* Calculates and maps academic week numbers for a specific semester period.
* 
* This method processes a single semester (fall or spring) and assigns academic
* week numbers to each working day in the semester. It follows these steps:
* 1. Finds the first Monday of the semester
* 2. Processes each week (Monday through Friday) until the semester end
* 3. Assigns week numbers to each working day (excluding holidays)
* 4. Determines whether each week is odd or even
* 
* The method handles the academic calendar's specific requirements:
* - Weeks start on Monday
* - Only weekdays (Monday-Friday) are assigned week numbers
* - Holidays are excluded from academic week assignments
* - Week numbers increment sequentially throughout the semester
* - Odd weeks are marked as "Impar", even weeks as "Par"
* 
* @param {Date} semesterStart - The start date of the semester
* @param {Date} semesterEnd - The end date of the semester
* @param {Map<string, { weekNumber: number, isOdd: boolean }>} calendar - The calendar map to update
* @param {number} startWeek - The starting week number (typically 1)
* @returns {void} This method doesn't return a value; it updates the provided calendar map
*/
  calculateSemesterWeeks(
    semesterStart: Date, 
    semesterEnd: Date, 
    calendar: Map<string, { weekNumber: number, isOdd: boolean }>,
    startWeek: number
  ): void {
    const firstDay = new Date(semesterStart);
    while (firstDay.getDay() !== 1) { // 1 = Monday
      firstDay.setDate(firstDay.getDate() + 1);
    }
    
    let currentDate = new Date(firstDay);
    let weekNumber = startWeek;
    
    while (currentDate <= semesterEnd) {
      for (let i = 0; i < 5; i++) { // Monday to Friday
        const day = new Date(currentDate);
        day.setDate(day.getDate() + i);
        
        if (day > semesterEnd) break;
        
        const isHoliday = this.isHoliday(day);

        if (!isHoliday) {
          const dateKey = this.formatDateKey(day);
          calendar.set(dateKey, {
            weekNumber,
            isOdd: weekNumber % 2 === 1
          });
        }
      }
      
      currentDate.setDate(currentDate.getDate() + 7);
      weekNumber++;
    }
  }

  generateDaysInRange(
    start: Date, 
    end: Date, 
    academicCalendar: Map<string, { weekNumber: number, isOdd: boolean }>
  ): CalendarDay[] {
    const days: CalendarDay[] = [];
    const currentDate = new Date(start);
    currentDate.setHours(0, 0, 0, 0);

    while (currentDate <= end) {
      const dayOfWeek = currentDate.getDay();
      
      const isHoliday = this.isHoliday(currentDate);
      const holidayName = this.getHolidayName(currentDate);
      
      const isWorkDay = dayOfWeek > 0 && dayOfWeek < 6 && !isHoliday;
      
      const dateKey = this.formatDateKey(currentDate);
      const academicWeek = academicCalendar.get(dateKey);
      
      let parityType: 'Par' | 'Impar' | null = null;
      let weekNumber: number | undefined = undefined;
      
      if (isWorkDay && academicWeek) {
        weekNumber = academicWeek.weekNumber;
        parityType = academicWeek.isOdd ? 'Impar' : 'Par';
      }

      days.push({
        date: new Date(currentDate),
        dayOfWeek: this.dayNames[dayOfWeek as keyof typeof this.dayNames],
        isWorkDay,
        parityType,
        isHoliday,
        holidayName,
        weekNumber
      });

      currentDate.setDate(currentDate.getDate() + 1);
    }

    return days;
  }

  applyDateFilter(): void {
    const formValues = this.filterForm.value;
    
    if (!formValues.startDate || !formValues.endDate) {
      this.filteredCalendarData = [...this.calendarData];
      return;
    }
    
    const startDate = new Date(formValues.startDate);
    const endDate = new Date(formValues.endDate);
    
    const currentStartYear = startDate.getFullYear();
    const currentEndYear = endDate.getFullYear();
    const previousStartYear = this.calendarData[0]?.date.getFullYear();
    const previousEndYear = this.calendarData[this.calendarData.length - 1]?.date.getFullYear();
    
    if (currentStartYear !== previousStartYear || currentEndYear !== previousEndYear) {
      this.loadHolidaysAndGenerateCalendar();
      return;
    }

    startDate.setHours(0, 0, 0, 0);
    endDate.setHours(23, 59, 59, 999);
    
    this.filteredCalendarData = this.calendarData.filter(day =>
      day.date >= startDate && day.date <= endDate
    );
  }

  formatDateKey(date: Date): string {
    return `${date.getFullYear()}-${date.getMonth()}-${date.getDate()}`;
  }

  isHoliday(date: Date): boolean {
    const key = this.formatDateKey(date);
    return this.holidays.has(key);
  }

  getHolidayName(date: Date): string | undefined {
    const key = this.formatDateKey(date);
    return this.holidays.get(key)?.localName;
  }

  formatDate(date: Date): string {
    return date.toLocaleDateString('ro-RO', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  }

  resetFilter(): void {
    this.filterForm.reset({
      startDate: new Date(2025, 4, 1),  // May 1, 2025
      endDate: new Date(2025, 4, 31)    // May 31, 2025
    });
    this.applyDateFilter();
  }
}