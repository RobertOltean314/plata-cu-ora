import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { UserService } from '../services/user-services/user.service';
import { Router } from '@angular/router';
import { RegisterRequest } from '../../models/register-request.model';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  standalone: false
})
export class RegisterComponent implements OnInit, OnDestroy  {
  model: RegisterRequest;
  confirmPassword: string = '';
  private registerSubscription?: Subscription;
  registrationError: string = '';

  constructor(private userService: UserService,
    private router: Router
  ) {
    this.model = {
      name: '',
      email: '',
      password: '',
      role: ''
    };
  }
  
  ngOnInit(): void {
    this.userService.user$.subscribe(user => {
      if (user) {
        this.router.navigate(['/account']);
      }
    });
  }

  isValid(): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return this.model.name.trim().length >= 3 &&
      emailRegex.test(this.model.email.trim()) &&
      this.model.password.trim().length >= 6 &&
      this.model.password === this.confirmPassword;
  }

  onFormSubmit() {
    if (!this.isValid()) {
      this.registrationError = 'Please complete all required fields correctly';
      return;
    }
    
    this.registrationError = '';
    this.registerSubscription = this.userService.register(this.model).subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: (error) => {
        this.registrationError = error.error?.message || 'Registration failed. Please try again.';
      }
    });
  }

  ngOnDestroy(): void {
    this.registerSubscription?.unsubscribe();
  }
}