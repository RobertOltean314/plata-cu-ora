import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { map, Observable } from 'rxjs';
import { UserService } from '../../features/services/user-services/user.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private userService: UserService, private router: Router) { }

  canActivate(): Observable<boolean | UrlTree> {
    return this.userService.getLoggedInUser().pipe(
      map(user => user ? true : this.router.createUrlTree(['/login']))
    );
  }
}