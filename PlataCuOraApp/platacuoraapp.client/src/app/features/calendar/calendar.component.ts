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
  calendarGenerated = false;
  calendarValidated = false;


  constructor(
    private fb: FormBuilder,
    private holidayService: HolidayService,
    private userService: UserService
  ) {
    const today = new Date();
    const firstDay = new Date();
    const lastDay = new Date();

    this.filterForm = this.fb.group({
      startDate: [],  // "yyyy-MM-dd"
      endDate: []      // "yyyy-MM-dd"
    }, { validators: this.validateDateRange });
  }

  validateDateRange(group: FormGroup): { [key: string]: any } | null {
    const start = group.get('startDate')?.value;
    const end = group.get('endDate')?.value;

    if (start && end && new Date(end) < new Date(start)) {
      return { dateRangeInvalid: true };
    }

    return null;
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
    if (this.filterForm.invalid) {
      alert('Intervalul de date este invalid.');
      return;
    }

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
        this.calendarGenerated = true;
        this.calendarValidated = false;
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
      this.calendarValidated = true;
    }
  }

  resetCalendar(): void {
    this.calendarData = [];
    this.startDateControl.setValue(null);
    this.endDateControl.setValue(null);
    this.status = 'Editare';
    this.calendarGenerated = false;
    this.calendarValidated = false;
  }

  toISODateString(date: Date): string {
    return date.toISOString().substring(0, 10);
  }

  generatePdf() {
    if (!this.userId) {
      alert('User ID not found!');
      return;
    }

    const zileLucrate: Date[] = this.calendarData
      .filter(day => day.workDayStatus === 'Zi lucratoare')
      .map(day => day.date);

    if (zileLucrate.length === 0) {
      alert('Nu există zile lucrate selectate.');
      return;
    }

    const firstDay = this.formatToMMDDYYYY(new Date(this.startDateControl.value));
    const lastDay = this.formatToMMDDYYYY(new Date(this.endDateControl.value));

    this.holidayService.genereazaDeclaratie(this.userId, zileLucrate, firstDay, lastDay).subscribe({
      next: (response) => {
        const blob = new Blob([response], { type: 'application/pdf' });
        const url = window.URL.createObjectURL(blob);
        window.open(url);  //  browserul face preview PDF
        // pt download
        // const a = document.createElement('a');
        // a.href = url;
        // a.download = 'declaratie.pdf';
        // a.click();
        // window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        alert('Eroare la generarea PDF: ' + err.message);
      }
    });
  }

  generateExcel() {
    if (!this.userId) {
      alert('User ID not found!');
      return;
    }

    const zileLucrate: Date[] = this.calendarData
      .filter(day => day.workDayStatus === 'Zi lucratoare')
      .map(day => day.date);

    if (zileLucrate.length === 0) {
      alert('Nu există zile lucrate selectate.');
      return;
    }

    const firstDay = this.formatToMMDDYYYY(new Date(this.startDateControl.value));
    const lastDay = this.formatToMMDDYYYY(new Date(this.endDateControl.value));

    this.holidayService.genereazaDeclaratieExcel(this.userId, zileLucrate, firstDay, lastDay).subscribe({
      next: (response) => {
        const blob = new Blob([response], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        const url = window.URL.createObjectURL(blob);

        const a = document.createElement('a');
        a.href = url;
        a.download = 'declaratie.xlsx';
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        alert('Eroare la generarea Excel: ' + err.message);
      }
    });
  }

  formatToMMDDYYYY(date: Date): string {
    const mm = ('0' + (date.getMonth() + 1)).slice(-2);
    const dd = ('0' + date.getDate()).slice(-2);
    const yyyy = date.getFullYear();
    return `${mm}.${dd}.${yyyy}`;
  }



  formatDateForBackend(dateStr: string): string {
    const [year, month, day] = dateStr.split('-'); // from yyyy-MM-dd
    return `${day}.${month}.${year}`;              // to dd.MM.yyyy
  }


}
