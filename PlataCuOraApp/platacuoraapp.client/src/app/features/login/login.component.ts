import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { UserService } from '../services/user-services/user.service';
import { LoginRequest } from '../../models/login-request.model';
import { AuthService } from '../../core/services/auth.service'; 

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: false
})
export class LoginComponent implements OnInit, OnDestroy {
  model: LoginRequest;
  private loginSubscription?: Subscription;
  private googleLoginSubscription?: Subscription;
  loginError: string = '';
  showPassword: boolean = false;
  isGoogleLoading: boolean = false;

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router
  ) {
    this.model = {
      email: '',
      password: ''
    };
  }

  ngOnInit(): void {
    this.userService.user$.subscribe(user => {
      if (user) {
        this.router.navigate(['/']);
      }
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
    const input = document.getElementById('loginPassword') as HTMLInputElement;
    if (input) {
      input.type = input.type === 'password' ? 'text' : 'password';
    }
  }

  onFormSubmit() {
    if (!this.model.email || !this.model.password) {
      this.loginError = 'Please enter email and password';
      return;
    }

    this.loginError = '';
    this.loginSubscription = this.userService.login(this.model).subscribe({
      next: (response) => {
        if (response.token && response.user) {
          this.userService.setLoggedInUser(response.user);
          sessionStorage.setItem('token', response.token);
          this.router.navigate(['/']);
        }
      },
      error: (error) => {
        this.loginError = error.error?.message || 'Login failed. Please check your credentials.';
      }
    });
  }

  async signInWithGoogle(): Promise<void> {
    this.isGoogleLoading = true;
    this.loginError = '';

    try {
      const googleAuthResult = await this.authService.signInWithGoogle();

      if (googleAuthResult && googleAuthResult.idToken) {
        this.googleLoginSubscription = this.userService.googleLogin(googleAuthResult.idToken).subscribe({
          next: (response) => {
            if (response.token && response.user) {
              this.userService.setLoggedInUser(response.user);
              sessionStorage.setItem('token', response.token);
              this.router.navigate(['/']);
            } else {
              this.loginError = 'Authentication failed - no user data received from backend';
            }
            this.isGoogleLoading = false;
          },
          error: (err) => {
            console.error('Error logging in with backend:', err);
            this.loginError = 'Login failed on backend.';
            this.isGoogleLoading = false;
          }
        });
      } else {
        throw new Error('Google authentication failed - no token received');
      }
    } catch (error: any) {
      console.error('Google sign-in failed:', error);
      this.loginError = error.message || 'Google sign-in failed. Please try again.';
      this.isGoogleLoading = false;
    }
  }

  ngOnDestroy(): void {
    this.loginSubscription?.unsubscribe();
    this.googleLoginSubscription?.unsubscribe();
  }
}
