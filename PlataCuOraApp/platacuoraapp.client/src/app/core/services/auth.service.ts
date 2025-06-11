import { Injectable } from '@angular/core';
import {
  GoogleAuthProvider,
  signInWithPopup,
  signOut,
  onAuthStateChanged,
  User
} from 'firebase/auth';
import { FirebaseService } from './firebase.service';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private userSubject = new BehaviorSubject<User | null>(null);
  public user$: Observable<User | null> = this.userSubject.asObservable();
  private apiUrl = 'http://localhost:5223/api'; 

  constructor(
    private firebaseService: FirebaseService,
    private http: HttpClient
  ) {
    // Listen for authentication state changes
    onAuthStateChanged(this.firebaseService.auth, (user) => {
      this.userSubject.next(user);
    });
  }

  async signInWithGoogle(): Promise<any> {
    try {
      const provider = new GoogleAuthProvider();
      provider.addScope('profile');
      provider.addScope('email');

      const result = await signInWithPopup(this.firebaseService.auth, provider);
      const idToken = await result.user.getIdToken();

      const response = await this.sendTokenToBackend(idToken);

      if (response && response.token) {
        sessionStorage.setItem('token', response.token);
      }

      return response;
    } catch (error) {
      console.error('Error signing in with Google:', error);
      throw error;
    }
  }

  async signOut(): Promise<void> {
    try {
      await signOut(this.firebaseService.auth);
    } catch (error) {
      console.error('Error signing out:', error);
      throw error;
    }
  }

  private async sendTokenToBackend(idToken: string): Promise<any> {
    try {
      return this.http.post(`${this.apiUrl}/user/google-login`,
        JSON.stringify(idToken), 
        {
          headers: { 'Content-Type': 'application/json' }
        }
      ).toPromise();
    } catch (error) {
      console.error('Error sending token to backend:', error);
      throw error;
    }
  }

  getCurrentUser(): User | null {
    return this.userSubject.value;
  }

  async getCurrentUserToken(): Promise<string | null> {
    const user = this.getCurrentUser();
    if (user) {
      return await user.getIdToken();
    }
    return null;
  }

  isAuthenticated(): boolean {
    return !!this.getCurrentUser();
  }
}
