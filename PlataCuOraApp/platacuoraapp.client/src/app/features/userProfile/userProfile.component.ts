import { Component, OnDestroy, OnInit } from '@angular/core';
import { UserService } from '../services/user-services/user.service';
import { Observable, Subscription, take } from 'rxjs';
import { User } from '../../models/user.model';
import { Router } from '@angular/router';
import { NgIf, AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-info',
  templateUrl: './userProfile.component.html',
  styleUrls: ['./userProfile.component.css'],
  standalone: false
})
export class UserProfileComponent implements OnInit, OnDestroy {
  user$: Observable<User | null> | undefined;
  private logoutSubscription?: Subscription;

  constructor(
    private userService: UserService,
    private router: Router
  ) { }

  ngOnInit() {
    this.user$ = this.userService.user$;
  }

  logout() {
    this.logoutSubscription = this.userService.logout().pipe(take(1)).subscribe({
      next: () => {
        sessionStorage.removeItem('token');
        this.router.navigate(['/login']);
      }
    });
  }

  changeCredentials() {
    // TODO: implement this functionallity
    console.log('Change credentials clicked');
  }

  ngOnDestroy() {
    this.logoutSubscription?.unsubscribe();
  }
}
