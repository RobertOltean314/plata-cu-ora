import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, switchMap, take } from 'rxjs';

import { OrarEntry } from '../../../models/orar-entry.model';
import { UserService } from '../../services/user-services/user.service'; // Try this path
import { environment } from '../../../environment/environment';

@Injectable({
  providedIn: 'root'
})
export class OrarService {
  private apiUrl = environment.apiBaseUrl + '/api/OrarUser';

  constructor(
    private http: HttpClient,
    private userService: UserService
  ) {}

  getAll(userId: string): Observable<OrarEntry[]> {
    return this.http.get<OrarEntry[]>(`${this.apiUrl}/${userId}`);
  }

  add(userId: string, entry: OrarEntry): Observable<string> {
    return this.http.post(`${this.apiUrl}/${userId}`, entry, { responseType: 'text' });
  }

  update(userId: string, oldEntry: OrarEntry, newEntry: OrarEntry): Observable<string> {
    return this.http.put(`${this.apiUrl}/${userId}`, { oldEntry, newEntry }, { responseType: 'text' });
  }

  delete(userId: string, entry: OrarEntry): Observable<string> {
    return this.http.request('delete', `${this.apiUrl}/${userId}`, {
      body: entry,
      responseType: 'text'
    });
  }
}
