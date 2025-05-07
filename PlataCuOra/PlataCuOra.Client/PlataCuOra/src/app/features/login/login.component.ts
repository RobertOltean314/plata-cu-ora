import { Component, OnDestroy, OnInit } from '@angular/core';
import { User } from 'src/app/models/user.model';
import { UserService } from '../services/user-services/user.service';
import { LoginRequest } from 'src/app/models/login-request.model';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit, OnDestroy  {
  model: LoginRequest;
  private loginSubscription?: Subscription;

  constructor(private userService: UserService,
    private router: Router
  ) {
    this.model = {
      email: '',
      password: ''
    };
  }
  ngOnInit(): void {
    if (this.userService.user$) {
      this.router.navigate(['/account']);
    }
  }

  onFormSubmit() {
    if (!this.model)
      return;
    this.loginSubscription = this.userService.login(this.model).subscribe({
      next: (response) => {
        if (response.token) {
          this.userService.setLoggedInUser(response.user);
          sessionStorage.setItem('token', response.token);
          this.router.navigate(['/account']);
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.loginSubscription?.unsubscribe();
  }
}