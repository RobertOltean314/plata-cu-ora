import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment/environment';
import { ParitateSaptamana } from '../../../models/paritate-sapt.model';

@Injectable({
  providedIn: 'root'
})
export class ParitateSaptService {
  private baseUrl = environment.apiBaseUrl + '/api/ParitateSapt';

  constructor(private http: HttpClient) {}

  getParitate(userId: string): Observable<ParitateSaptamana[]> {
    return this.http.get<ParitateSaptamana[]>(`${this.baseUrl}/${userId}`);
  }

  addOrUpdate(userId: string, data: ParitateSaptamana[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/${userId}`, data);
  }

  update(userId: string, oldEntry: ParitateSaptamana, newEntry: ParitateSaptamana): Observable<any> {
    return this.http.put(`${this.baseUrl}/${userId}`, { oldEntry, newEntry });
  }

  delete(userId: string, entry: ParitateSaptamana): Observable<any> {
    return this.http.request('delete', `${this.baseUrl}/${userId}`, { body: entry });
  }

  replaceAll(userId: string, entries: ParitateSaptamana[]): Observable<any> {
    return this.http.put(`${this.baseUrl}/replace/${userId}`, entries);
  }
 
}

