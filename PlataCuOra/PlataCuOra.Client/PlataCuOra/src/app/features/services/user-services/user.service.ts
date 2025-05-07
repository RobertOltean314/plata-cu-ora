import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, map, Observable, timer } from 'rxjs';
import { User } from 'src/app/models/user.model';
import { LoginRequest } from 'src/app/models/login-request.model';
import { environment } from 'src/app/environment/environment';
import { RegisterRequest } from 'src/app/models/register-request.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromSession());
  public user$: Observable<User | null> = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) { }

  setLoggedInUser(user: User) {
    sessionStorage.setItem('user', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  getLoggedInUser(): Observable<User | null> {
    return this.user$;
  }

  login (model: LoginRequest) : Observable<any> {
    return this.http.post<any>(`${environment.apiBaseUrl}/api/user/login`, model);
  }

  register (model: RegisterRequest) : Observable<void> {
    return this.http.post<void>(`${environment.apiBaseUrl}/api/user/register`, model);
  }

  logout(): Observable<void> {
    sessionStorage.removeItem('user');
    this.currentUserSubject.next(null);
    return timer(0).pipe(map(() => {}));
  }

  private getUserFromSession(): User | null {
    const userJson = sessionStorage.getItem('user');

    if (userJson === null || userJson === undefined) {
      return null;
    }
    return JSON.parse(userJson) as User;
  }
}
