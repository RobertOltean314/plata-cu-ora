import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { UserService } from '../services/user-services/user.service';
import { LoginRequest } from '../../models/login-request.model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: false
})
export class LoginComponent implements OnInit, OnDestroy  {
  model: LoginRequest;
  private loginSubscription?: Subscription;
  loginError: string = '';

  constructor(private userService: UserService,
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
        this.router.navigate(['/account']);
      }
    });
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
          this.router.navigate(['/account']);
        }
      },
      error: (error) => {
        this.loginError = error.error?.message || 'Login failed. Please check your credentials.';
      }
    });
  }

  ngOnDestroy(): void {
    this.loginSubscription?.unsubscribe();
  }
}
