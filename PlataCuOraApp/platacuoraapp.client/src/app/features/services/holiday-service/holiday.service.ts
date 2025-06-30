import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { CalendarDay } from '../../../models/calendarDay.model';

export interface PublicHoliday {
  date: string;
  localName: string;
  name: string;
  countryCode: string;
  fixed: boolean;
  global: boolean;
  counties: string[] | null;
  launchYear: number | null;
  types: string[];
}

@Injectable({ providedIn: 'root' })
export class HolidayService {
  private cache = new Map<number, Observable<PublicHoliday[]>>();

  constructor(private http: HttpClient) {}

  getPublicHolidays(year: number): Observable<PublicHoliday[]> {
    if (this.cache.has(year)) {
      return this.cache.get(year)!;
    }
    const request = this.http.get<PublicHoliday[]>(`/api/Holidays/${year}`)
      .pipe(shareReplay(1));
    this.cache.set(year, request);
    return request;
  }

  getHolidayDates(year: number): Observable<Date[]> {
    return this.getPublicHolidays(year).pipe(
      map(holidays => holidays.map(h => new Date(h.date)))
    );
  }

  // NEW: Get working days for a user and interval
  getWorkingDays(userId: string, start: string, end: string): Observable<CalendarDay[]> {
    return this.http.get<CalendarDay[]>(`/api/WorkingDays/${userId}/${start}/${end}`);
  }
  

  genereazaDeclaratie(userId: string, zileLucrate: Date[], firstDay: string, lastDay: string) {
    const url = `/api/Declaratie/genereaza?userId=${userId}&firstDay=${firstDay}&lastDay=${lastDay}`;
    return this.http.post(url, zileLucrate, { responseType: 'blob' });
  }

  genereazaDeclaratieExcel(userId: string, zileLucrate: Date[], startDate: string, endDate: string): Observable<Blob> {
    const url = `/api/declaratie/genereaza-excel?userId=${userId}&firstDay=${startDate}&lastDay=${endDate}`;
    return this.http.post(url, zileLucrate, { responseType: 'blob' });
  }

}