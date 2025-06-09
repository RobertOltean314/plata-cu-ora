import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environment/environment';

export interface UserInfo {
  declarant: string;
  tip: string;
  directorDepartament: string;
  decan: string;
  universitate: string;
  facultate: string;
  departament: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserInfoService {
  private apiUrl = `${environment.apiBaseUrl}/api/infoUser/extra-info`;

  constructor(private http: HttpClient) {}

  getUserInfo(userId: string): Observable<UserInfo> {
    return this.http.get<UserInfo>(`${this.apiUrl}/${userId}`);
  }

  saveUserInfo(userId: string, info: UserInfo): Observable<any> {
    return this.http.post(`${this.apiUrl}/${userId}`, info);
  }
}