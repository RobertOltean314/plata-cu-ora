// navbar.component.ts
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { UserService } from '../../features/services/user-services/user.service';
import { UserInfoService, UserInfo } from '../../features/services/user-info/user-info.service';


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

  userInfo: UserInfo = {
    declarant: '',
    tip: '',
    directorDepartament: '',
    decan: '',
    universitate: '',
    facultate: '',
    departament: ''
  };

  successMessage: string = '';
  errorMessage: string = '';

  constructor(
    private userService: UserService,
    private userInfoService: UserInfoService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userService.getLoggedInUser().subscribe(user => {
      this.user = user;
      /*if (user && user.id) {
        this.userInfoService.getUserInfo(String(user.id)).subscribe({
          next: (info: UserInfo) => this.userInfo = info,
          error: err => console.error('Error fetching user info:', err)
        });
      }*/
    });
  }

  openUserInfoModal(): void {
    this.userInfo = {
      declarant: '',
      tip: '',
      directorDepartament: '',
      decan: '',
      universitate: '',
      facultate: '',
      departament: ''
    };
    this.successMessage = '';
    this.errorMessage = '';
  }

  saveUserInfo(): void {
    this.successMessage = '';
    this.errorMessage = '';
    if (this.user && this.user.id) {
      this.userInfoService.saveUserInfo(String(this.user.id), this.userInfo).subscribe({
        next: () => {
          this.successMessage = 'User information saved successfully.';
          setTimeout(() => {
            this.successMessage = '';
          }
          , 1200); // Clear message after 3 seconds
        },
        error: err => {
          this.errorMessage = err.error?.message || 'User info already exists or could not be saved!';
        }
      });
    }
  }

  closeModal() {
    const modal = document.getElementById('userInfoModal');
    if (modal) {
      (window as any).bootstrap.Modal.getInstance(modal)?.hide();
    }
  }

  logout(): void {
    this.userService.logout().subscribe(() => {
      this.router.navigate(['/login']);
    });
  }
}