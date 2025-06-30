import { Injectable } from '@angular/core';
//import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, catchError, map, Observable, of, throwError, timer } from 'rxjs';
import { User } from '../../../models/user.model';
import { LoginRequest } from '../../../models/login-request.model';
import { environment } from '../../../environment/environment';
import { RegisterRequest } from '../../../models/register-request.model';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromSession());
  public user$: Observable<User | null> = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    //this.validateToken();
  }
 
  getUserId(): string | null {
    const id = this.currentUserSubject.value?.id ?? null;
    return id !== null ? id.toString() : null;
  }

  setLoggedInUser(user: User) {
    sessionStorage.setItem('user', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  getLoggedInUser(): Observable<User | null> {
    return this.user$;
  }

  login(model: LoginRequest): Observable<any> {
    return this.http.post<any>(`${environment.apiBaseUrl}/api/user/login`, model)
      .pipe(
        catchError(this.handleError)
      );
  }

  googleLogin(idToken: string): Observable<any> {
    return this.http.post<any>(
      `${environment.apiBaseUrl}/api/user/google-login`,
      { idToken },
      {
        headers: { 'Content-Type': 'application/json' }
      }
    )
      .pipe(
        map((response: any) => {
          if (response.user && response.token) {
            sessionStorage.setItem('token', response.token);
            this.setLoggedInUser(response.user);
          }
          return response;
        }),
        catchError(this.handleError)
      );
  }

  register(model: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${environment.apiBaseUrl}/api/user/register`, model)
      .pipe(
        catchError(this.handleError)
      );
  }

  logout(): Observable<void> {
    sessionStorage.removeItem('user');
    sessionStorage.removeItem('token');
    this.currentUserSubject.next(null);
    return timer(0).pipe(map(() => {}));
  }

  //validateToken(): void {
  //  const token = sessionStorage.getItem('token');
  //  if (!token) {
  //    this.logout();
  //    return;
  //  }

  //  this.http.post<{valid: boolean}>(`${environment.apiBaseUrl}/api/user/verify-token`, { token })
  //    .pipe(catchError(() => of({ valid: false })))
  //    .subscribe(response => {
  //      if (!response.valid) {
  //        this.logout();
  //      }
  //    });
  //}

  // Add method to get current user info from backend (useful for token validation)
  getCurrentUserFromBackend(): Observable<User> {
    return this.http.get<User>(`${environment.apiBaseUrl}/api/user/me`)
      .pipe(
        catchError(this.handleError)
      );
  }

  private getUserFromSession(): User | null {
    const userJson = sessionStorage.getItem('user');

    if (userJson === null || userJson === undefined) {
      return null;
    }
    
    try {
      return JSON.parse(userJson) as User;
    } catch (e) {
      console.error('Error parsing user from session storage', e);
      sessionStorage.removeItem('user');
      return null;
    }
  }

  private handleError(error: HttpErrorResponse) {
    console.error('An error occurred:', error);
    return throwError(() => error);
  }
}
