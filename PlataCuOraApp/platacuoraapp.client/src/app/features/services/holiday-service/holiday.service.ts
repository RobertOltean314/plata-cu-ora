import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';

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

  @Injectable({providedIn: 'root'})
export class HolidayService {
    // We'll use this later to cache the results of the HTTP requests
    private cache = new Map<number, Observable<PublicHoliday[]>>();

    constructor(private http: HttpClient) {}

    getPublicHolidays(year: number): Observable<PublicHoliday[]> {
        if (this.cache.has(year)) {
            return this.cache.get(year)!;
        }
        // We put the result from the HTTP request in the cache
        const request = this.http.get<PublicHoliday[]>(`/api/Holidays/${year}`)
        .pipe(
          shareReplay(1)
        );
        
      this.cache.set(year, request);
      return request;
    }
    
    getHolidayDates(year: number): Observable<Date[]> {
        return this.getPublicHolidays(year).pipe(
          map(holidays => holidays.map(h => new Date(h.date)))
        );
      }
}