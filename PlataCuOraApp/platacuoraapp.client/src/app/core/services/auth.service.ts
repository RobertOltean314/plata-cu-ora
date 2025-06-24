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
import { environment } from '../../environment/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private firebaseUserSubject = new BehaviorSubject<User | null>(null);
  public firebaseUser$: Observable<User | null> = this.firebaseUserSubject.asObservable();
  
  // Keep this for compatibility with auth guard if needed
  public user$: Observable<User | null> = this.firebaseUserSubject.asObservable();

  constructor(
    private firebaseService: FirebaseService,
    private http: HttpClient
  ) {
    // Listen for Firebase authentication state changes
    onAuthStateChanged(this.firebaseService.auth, (user) => {
      this.firebaseUserSubject.next(user);
    });
  }

  async signInWithGoogle(): Promise<any> {
    try {
      const provider = new GoogleAuthProvider();
      provider.addScope('profile');
      provider.addScope('email');

      const result = await signInWithPopup(this.firebaseService.auth, provider);
      const idToken = await result.user.getIdToken();

      // Send token to backend and get your app's user data and JWT
      const response = await this.sendTokenToBackend(idToken);
      return response;
    } catch (error: any) {
      console.error('Error signing in with Google:', error);
      
      // Handle specific Firebase errors
      if (error.code === 'auth/popup-closed-by-user') {
        throw new Error('Sign-in was cancelled');
      } else if (error.code === 'auth/popup-blocked') {
        throw new Error('Popup was blocked by your browser. Please allow popups and try again.');
      } else {
        throw new Error('Google sign-in failed. Please try again.');
      }
    }
  }

  async signOut(): Promise<void> {
    try {
      await signOut(this.firebaseService.auth);
      this.firebaseUserSubject.next(null);
    } catch (error: any) {
      console.error('Error signing out:', error);
      throw error;
    }
  }

  private async sendTokenToBackend(idToken: string): Promise<any> {
    try {
      // Send the token as a raw string to match your backend signature [FromBody] string idToken
      const response = await this.http.post(`${environment.apiBaseUrl}/api/user/google-login`, 
        JSON.stringify(idToken), // Send as raw string to match your controller
        {
          headers: { 'Content-Type': 'application/json' }
        }
      ).toPromise();
      
      return response;
    } catch (error: any) {
      console.error('Error sending token to backend:', error);
      throw error;
    }
  }

  getCurrentFirebaseUser(): User | null {
    return this.firebaseUserSubject.value;
  }

  async getCurrentUserToken(): Promise<string | null> {
    const user = this.getCurrentFirebaseUser();
    if (user) {
      return await user.getIdToken();
    }
    return null;
  }

  isFirebaseAuthenticated(): boolean {
    return !!this.getCurrentFirebaseUser();
  }
}