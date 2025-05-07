import { Component, OnDestroy, OnInit } from '@angular/core';
import { UserService } from '../services/user-services/user.service';
import { Observable, Subscription, take } from 'rxjs';
import { User } from 'src/app/models/user.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-info',
  templateUrl: './info.component.html',
  styleUrls: ['./info.component.css']
})
export class InfoComponent implements OnInit, OnDestroy {
  user$: Observable<User | null> | undefined;
  private logoutSubscription?: Subscription;

  constructor(private userService: UserService, private router: Router) {}

  ngOnInit() {
    this.user$ = this.userService.user$;
  }

  logout(){
    this.logoutSubscription = this.userService.logout().pipe(take(1)).subscribe({
      next: () => {
        this.router.navigate(['/login']);
      }
    });
  }

  ngOnDestroy() {
      this.logoutSubscription?.unsubscribe();
  }
}