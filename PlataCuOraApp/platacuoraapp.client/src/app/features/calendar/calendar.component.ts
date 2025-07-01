import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormControl } from '@angular/forms';
import { HolidayService } from '../services/holiday-service/holiday.service';
import { UserService } from '../services/user-services/user.service';
import { InfoUserDTO, InfoUserService } from '../services/userInfo-services/user-info.service';
import { OrarService } from '../services/orar-services/orar-service.service';
import { OrarEntry } from '../../models/orar-entry.model';
import { ParitateSaptService } from '../services/partitate-sapt-services/paritate-sapt.service';
import { ParitateSaptamana } from '../../models/paritate-sapt.model';

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


  successMessage: string = '';
  errorMessage: string = '';

  //profile
  profiles: InfoUserDTO[] = [];
  selectedProfile?: InfoUserDTO;

  //orar
  orar: OrarEntry[] = [];

  //struct sem
  paritati: ParitateSaptamana[] = [];

  constructor(
    private fb: FormBuilder,
    private holidayService: HolidayService,
    private userService: UserService,
    private infoUserService: InfoUserService,
    private orarService: OrarService,
    private paritateService: ParitateSaptService
  ) {
    const today = new Date();
    const firstDay = new Date();
    const lastDay = new Date();

    this.filterForm = this.fb.group({
      startDate: [],  // "yyyy-MM-dd"
      endDate: []      // "yyyy-MM-dd"
    }, { validators: this.validateDateRange });

    this.userId = this.userService.getUserId();
    if (this.userId) {
      this.loadProfiles();
      this.loadOrar();
      this.loadParitati();
    }
  }

  validateDataCalendar() {
    const hasProfiles = this.profiles.length > 0;
    const hasOrar = this.orar.length > 0;
    const hasParitati = this.paritati.length > 0;
    const hasActiveProfile = this.profiles.some(p => p.isActive === true);

    if (!hasProfiles || !hasOrar || !hasParitati || !hasActiveProfile) {
      let messages = [];

      if (!hasProfiles) messages.push('Nu există profil(e) disponibile.');
      if (!hasOrar) messages.push('Nu există date în orar.');
      if (!hasParitati) messages.push('Structura semestrului nu este completă.');
      if (!hasActiveProfile) messages.push('Niciun profil nu este activat.');

      alert('Validarea a eșuat:\n' + messages.join('\n'));
      return; 
    }
    console.log('Date calendar validate cu succes!');
  }


  loadProfiles() {
    if (!this.userId) return;
    this.infoUserService.getAllInfo(this.userId).subscribe({
      next: (profiles) => {
        this.profiles = profiles;
        // Only select the active profile, do not default to the first one
        this.selectedProfile = profiles.find(p => p.isActive) || undefined;
      },
      error: () => {
        this.errorMessage = 'Could not load profiles.';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  loadOrar() {
    if (!this.userId) return;
    this.orarService.getAll(this.userId).subscribe({
      next: (data) => {
        this.orar = data;
      },
      error: (err) => {
        console.error('Eroare la încărcare orar:', err);
      }
    });
  }

  loadParitati() {
    if (this.userId) {
      this.paritateService.getParitate(this.userId).subscribe(data => {
        this.paritati = data;
      });
    } else {
      console.warn('UserId is null, cannot load data');
    }
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
    this.validateDataCalendar();
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
