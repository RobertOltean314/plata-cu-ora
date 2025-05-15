// navbar.component.ts
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { UserService } from '../../features/services/user-services/user.service';


@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
  standalone: false,  
})
export class NavbarComponent implements OnInit {
  isMenuCollapsed = true;
  user: any;
  notificationCount = 0;

  constructor(
    private userService: UserService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userService.getLoggedInUser().subscribe(user => {
      this.user = user;
    });
    
  }

  logout(): void {
    this.userService.logout().subscribe(() => {
      this.router.navigate(['/login']);
    });
  }
}