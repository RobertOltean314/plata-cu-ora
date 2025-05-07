import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { UserService } from '../services/user-services/user.service';
import { Router } from '@angular/router';
import { RegisterRequest } from 'src/app/models/register-request.model';
import { User } from 'src/app/models/user.model';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit, OnDestroy  {
  model: RegisterRequest;
  confirmPassword: string = '';
  private registerSubscription?: Subscription;

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
  }

  isValid() : boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return this.model.name.trim().length >= 3 &&
      emailRegex.test(this.model.email.trim()) &&
      this.model.password.trim().length >= 6 &&
      this.model.role.trim() !== '' &&
      this.model.password === this.confirmPassword;
  }

  onFormSubmit() {
    if (!this.model)
      return;
    this.registerSubscription = this.userService.register(this.model).subscribe({
      next: () => {
        this.router.navigate(['/login']);
      }
    });
  }

  ngOnDestroy(): void {
    this.registerSubscription?.unsubscribe();
  }
}