import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormControl } from '@angular/forms';
import { HolidayService } from '../services/holiday-service/holiday.service';
import { UserService } from '../services/user-services/user.service';

export interface CalendarDay {
  date: Date;
  dayOfWeek: string;
  isWorkingDay: boolean;
  parity: string;
  workDayStatus?: string;
  parityType?: string;
}

@Component({
  standalone: true,
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.css'],
  imports: [CommonModule, FormsModule, ReactiveFormsModule]
})
export class CalendarComponent implements OnInit {
  calendarData: CalendarDay[] = [];
  filterForm: FormGroup;
  loading = false;
  status: 'Editare' | 'Validat' = 'Editare';
  userId: string | null = null;

  constructor(
    private fb: FormBuilder,
    private holidayService: HolidayService,
    private userService: UserService
  ) {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    this.filterForm = this.fb.group({
      startDate: [this.toISODateString(firstDay)],  // "yyyy-MM-dd"
      endDate: [this.toISODateString(lastDay)]      // "yyyy-MM-dd"
    });
  }

  get startDateControl(): FormControl {
    return this.filterForm.get('startDate') as FormControl;
  }

  get endDateControl(): FormControl {
    return this.filterForm.get('endDate') as FormControl;
  }

  ngOnInit(): void {
    this.userId = this.userService.getUserId();
    this.generateCalendar();
  }

  generateCalendar(): void {
    this.loading = true;
    this.holidayService.getWorkingDays(
      this.userId!,
      this.startDateControl.value,
      this.endDateControl.value
    ).subscribe({
      next: (days) => {
        console.log('Zile primite:', days);
        this.calendarData = days.map(d => ({
          ...d,
          date: new Date(d.date),
          workDayStatus: d.isWorkingDay ? 'Zi lucratoare' : 'Zi nelucratoare',
          parityType: d.parity
        }));
        this.loading = false;
        this.status = 'Editare';
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onWorkDayChange(day: CalendarDay, value: string) {
    day.workDayStatus = value as 'Zi lucratoare' | 'Zi nelucratoare' | '';
    day.isWorkingDay = value === 'Zi lucratoare';
    this.status = 'Editare';
  }


  getParityDisplay(parity: string): string {
    if (parity === 'par') return 'Pară';
    if (parity === 'impar') return 'Impară';
    return '';
  }

  onDateChange() {
    this.status = 'Editare';
  }

  validateCalendar() {
    if (this.calendarData.length > 0) {
      this.status = 'Validat';
    }
  }

  toISODateString(date: Date): string {
    return date.toISOString().substring(0, 10);
  }


  generatePdf() {
  if (!this.userId) {
    alert('User ID not found!');
    return;
  }

  // Extrage doar zilele lucrătoare (workDayStatus === 'Zi lucratoare')
  const zileLucrate: Date[] = this.calendarData
    .filter(day => day.workDayStatus === 'Zi lucratoare')
    .map(day => day.date);

  if (zileLucrate.length === 0) {
    alert('Nu există zile lucrate selectate.');
    return;
  }

  // Trimite cererea POST către backend
  this.holidayService.genereazaDeclaratie(this.userId, zileLucrate).subscribe({
    next: (response) => {
      // Response este un blob PDF => îl descărcăm
      const blob = new Blob([response], { type: 'application/pdf' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'declaratie.pdf';
      a.click();
      window.URL.revokeObjectURL(url);
    },
    error: (err) => {
      alert('Eroare la generarea PDF: ' + err.message);
    }
  });
}

}
