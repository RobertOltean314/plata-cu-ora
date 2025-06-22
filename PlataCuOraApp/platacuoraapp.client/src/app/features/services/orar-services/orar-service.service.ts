import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, switchMap, take } from 'rxjs';

import { OrarEntry } from '../../../models/orar-entry.model';
import { UserService } from '../../services/user-services/user.service'; // Try this path

@Injectable({
  providedIn: 'root'
})
export class OrarService {
  private apiUrl = 'https://localhost:49219/api/OrarUser';

  constructor(
    private http: HttpClient,
    private userService: UserService
  ) {}

  getAll(): Observable<OrarEntry[]> {
    return this.userService.getLoggedInUser().pipe(
      take(1),
      switchMap(user => {
        if (!user?.id) {
          throw new Error('User not logged in');
        }
        return this.http.get<OrarEntry[]>(`${this.apiUrl}/${user.id}`);
      })
    );
  }

  add(entry: OrarEntry): Observable<string> {
    return this.userService.getLoggedInUser().pipe(
      take(1),
      switchMap(user => {
        if (!user?.id) {
          throw new Error('User not logged in');
        }
        return this.http.post(`${this.apiUrl}/${user.id}`, entry, { responseType: 'text' });
      })
    );
  }

  update(oldEntry: OrarEntry, newEntry: OrarEntry): Observable<string> {
    return this.userService.getLoggedInUser().pipe(
      take(1),
      switchMap(user => {
        if (!user?.id) {
          throw new Error('User not logged in');
        }
        return this.http.put(`${this.apiUrl}/${user.id}`, { oldEntry, newEntry }, { responseType: 'text' });
      })
    );
  }

  delete(entry: OrarEntry): Observable<string> {
    return this.userService.getLoggedInUser().pipe(
      take(1),
      switchMap(user => {
        if (!user?.id) {
          throw new Error('User not logged in');
        }
        return this.http.request('delete', `${this.apiUrl}/${user.id}`, { 
          body: entry, 
          responseType: 'text' 
        });
      })
    );
  }
}