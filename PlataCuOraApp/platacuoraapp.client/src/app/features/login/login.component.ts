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
  loginError: string = '';
  showPassword: boolean = false;
  isGoogleLoading: boolean = false; // Add this

  constructor(
    private userService: UserService,
    private authService: AuthService, // Add this
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
        if (response.token) {
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

  // Add this method for Google login
  async signInWithGoogle(): Promise<void> {
    this.isGoogleLoading = true;
    this.loginError = '';

    try {
      await this.authService.signInWithGoogle();
      // The AuthService handles the backend call and user state
      // Just navigate to home after successful authentication
      this.router.navigate(['/']);
    } catch (error: any) {
      this.loginError = error.message || 'Google sign-in failed. Please try again.';
    } finally {
      this.isGoogleLoading = false;
    }
  }

  ngOnDestroy(): void {
    this.loginSubscription?.unsubscribe();
  }
}
