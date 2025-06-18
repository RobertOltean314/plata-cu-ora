import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { OrarEntry } from '../../../models/orar-entry.model'; 

@Injectable({
  providedIn: 'root'
})
export class OrarService {
  private apiUrl = 'https://localhost:5001/api/OrarUser'; // sau ce URL ai tu la backend
  private userId = 'ID_UL_TAU'; // ID-ul utilizatorului curent - schimbă-l cu cel real

  constructor(private http: HttpClient) {}

  getAll(): Observable<OrarEntry[]> {
    return this.http.get<OrarEntry[]>(`${this.apiUrl}/${this.userId}`);
  }

  add(entry: OrarEntry): Observable<any> {
    return this.http.post(`${this.apiUrl}/${this.userId}`, entry);
  }

  update(oldEntry: OrarEntry, newEntry: OrarEntry): Observable<any> {
    return this.http.put(`${this.apiUrl}/${this.userId}`, { oldEntry, newEntry });
  }

  delete(entry: OrarEntry): Observable<any> {
    return this.http.request('delete', `${this.apiUrl}/${this.userId}`, { body: entry });
  }
}
