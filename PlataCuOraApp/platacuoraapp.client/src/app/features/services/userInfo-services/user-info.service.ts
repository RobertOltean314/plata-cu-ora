import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface InfoUserDTO {
  declarant: string;
  tip: string;
  directorDepartament: string;
  decan: string;
  universitate: string;
  facultate: string;
  departament: string;
  isActive: boolean;
}

export interface UpdateInfoRequestDTO {
  old: InfoUserDTO;
  new: InfoUserDTO;
}

@Injectable({
  providedIn: 'root'
})
export class InfoUserService {
  private apiUrl = 'api/InfoUser'; // Adjust base URL as needed

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = sessionStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  getAllInfo(userId: string): Observable<InfoUserDTO[]> {
    return this.http.get<InfoUserDTO[]>(`${this.apiUrl}/all/${userId}`, {
      headers: this.getHeaders()
    });
  }

  addInfo(userId: string, info: InfoUserDTO): Observable<any> {
    console.log('Adding info:', info);
    console.log('User ID:', userId);
    if (!info.declarant || !info.tip || !info.directorDepartament || !info.decan || !info.universitate || !info.facultate || !info.departament) {
      console.error('Invalid info data:', info);
      // Return an observable error if data is invalid
      return new Observable(observer => {
        observer.error('Invalid info data');
      });
    }
    return this.http.post(`${this.apiUrl}/add/${userId}`, info, {
      headers: this.getHeaders()
    });
  }

  updateInfo(userId: string, oldInfo: InfoUserDTO, newInfo: InfoUserDTO): Observable<any> {
    const request: UpdateInfoRequestDTO = { old: oldInfo, new: newInfo };
    return this.http.put(`${this.apiUrl}/update/${userId}`, request, {
      headers: this.getHeaders()
    });
  }

  deleteInfo(userId: string, info: InfoUserDTO): Observable<any> {
    return this.http.delete(`${this.apiUrl}/delete/${userId}`, {
      headers: this.getHeaders(),
      body: info
    });
  }

  setActive(userId: string, info: InfoUserDTO): Observable<any> {
    return this.http.post(`${this.apiUrl}/set-active/${userId}`, info, {
      headers: this.getHeaders()
    });
  }

  unsetActive(userId: string, info: InfoUserDTO): Observable<any> {
    return this.http.post(`${this.apiUrl}/unset-active/${userId}`, info, {
      headers: this.getHeaders()
    });
  }

  getActiveInfo(userId: string): Observable<InfoUserDTO> {
    return this.http.get<InfoUserDTO>(`${this.apiUrl}/${userId}/active-info`, {
      headers: this.getHeaders()
    });
  }

  addActiveInfoToDb(userId: string): Observable<InfoUserDTO> {
    return this.http.post<InfoUserDTO>(`${this.apiUrl}/${userId}/add-active-info`, {}, {
      headers: this.getHeaders()
    });
  }

  getInfoUserFromDb(userId: string): Observable<InfoUserDTO> {
    return this.http.get<InfoUserDTO>(`${this.apiUrl}/${userId}/info-user`, {
      headers: this.getHeaders()
    });
  }
}