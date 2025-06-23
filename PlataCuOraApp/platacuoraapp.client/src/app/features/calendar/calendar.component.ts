import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormControl } from '@angular/forms';
import { HolidayService } from '../services/holiday-service/holiday.service';
import { CalendarDay } from '../../models/calendarDay.model';

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
  userId = '7aqWqwVzasQflpnQDTr8eiuM8S92'; 

  constructor(
    private fb: FormBuilder,
    private holidayService: HolidayService
  ) {
    this.filterForm = new FormGroup({
      startDate: new FormControl({ value: '2025-02-01', disabled: false }),
      endDate: new FormControl({ value: '2025-02-28', disabled: false })
    });
  }

  get startDateControl(): FormControl {
    return this.filterForm.get('startDate') as FormControl;
  }
  get endDateControl(): FormControl {
    return this.filterForm.get('endDate') as FormControl;
  }

  ngOnInit(): void {
    this.generateCalendar();
  }

  generateCalendar(): void {
    this.loading = true;
    this.status = 'Editare'; // Reset to editing on generate
    const start = this.startDateControl.value;
    const end = this.endDateControl.value;
    this.holidayService.getWorkingDays(this.userId, start, end)
      .subscribe({
        next: (days) => {
          this.calendarData = days.map((d, idx) => ({
            ...d,
            workDayStatus: d.isWorkDay ? 'Zi lucratoare' : ''
          }));
          this.loading = false;
        },
        error: () => { this.loading = false; }
      });
  }

  onWorkDayChange(day: CalendarDay, value: string) {
    day.workDayStatus = value as '' | 'Zi lucratoare' | 'Zi nelucratoare';
    this.status = 'Editare'; // Set to editing on any change
  }

  onDateChange() {
    this.status = 'Editare'; 
  }
  
  validateCalendar() {
    if (this.calendarData.length > 0) {
      this.status = 'Validat';
    }
  }

  formatDate(date: Date): string {
    const d = new Date(date);
    return d.toLocaleDateString('ro-RO', { year: 'numeric', month: '2-digit', day: '2-digit' });
  }
}