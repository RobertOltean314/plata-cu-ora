import { Component, OnDestroy, OnInit } from '@angular/core';
import { UserService } from '../services/user-services/user.service';
import { Observable, Subscription, take } from 'rxjs';
import { User } from '../../models/user.model';
import { Router } from '@angular/router';
import { InfoUserService, InfoUserDTO } from '../services/userInfo-services/user-info.service';

@Component({
  selector: 'app-info',
  templateUrl: './userProfile.component.html',
  styleUrls: ['./userProfile.component.css'],
  standalone: false
})
export class UserProfileComponent implements OnInit, OnDestroy {
  user$: Observable<User | null> | undefined;
  private logoutSubscription?: Subscription;

  profiles: InfoUserDTO[] = [];
  selectedProfile?: InfoUserDTO;
  editingProfile?: InfoUserDTO;
  userId: string = '';

  constructor(
    private userService: UserService,
    private router: Router,
    private infoUserService: InfoUserService
  ) { }

  ngOnInit() {
    this.user$ = this.userService.user$;
    this.user$.pipe(take(1)).subscribe(user => {
      if (user) {
        this.userId = String(user.id);
        this.loadProfiles();
      }
    });
  }

  loadProfiles() {
    if (!this.userId) return;
    this.infoUserService.getAllInfo(this.userId).subscribe(profiles => {
      this.profiles = profiles;
      this.selectedProfile = profiles.find(p => p.isActive) || profiles[0];
    });
  }

  onProfileSelect(profile: InfoUserDTO) {
    this.selectedProfile = profile;
  }

  setActiveProfile() {
    if (!this.selectedProfile) return;
    this.infoUserService.setActive(this.userId, this.selectedProfile).subscribe(() => {
      this.loadProfiles();
    });
  }

  editProfile(profile: InfoUserDTO) {
    this.editingProfile = { ...profile };
  }

  addNewProfile() {
    this.editingProfile = {
      declarant: '',
      tip: '',
      directorDepartament: '',
      decan: '',
      universitate: '',
      facultate: '',
      departament: '',
      isActive: false
    };
  }

  saveProfile() {
    if (!this.editingProfile) return;
    if (this.selectedProfile && this.editingProfile && this.selectedProfile !== this.editingProfile) {
      // Update existing
      this.infoUserService.updateInfo(this.userId, this.selectedProfile, this.editingProfile).subscribe(() => {
        this.editingProfile = undefined;
        this.loadProfiles();
      });
    } else {
      // Add new
      this.infoUserService.addInfo(this.userId, this.editingProfile).subscribe(() => {
        this.editingProfile = undefined;
        this.loadProfiles();
      });
    }
  }

  cancelEdit() {
    this.editingProfile = undefined;
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