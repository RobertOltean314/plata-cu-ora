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
  userDis: any;
  private logoutSubscription?: Subscription;

  profiles: InfoUserDTO[] = [];
  selectedProfile?: InfoUserDTO;
  editingProfile?: InfoUserDTO;
  userId: string = '';
  successMessage: string = '';
  errorMessage: string = '';

  constructor(
    private userService: UserService,
    private router: Router,
    private infoUserService: InfoUserService
  ) { }

  ngOnInit() {
    this.userService.getLoggedInUser().subscribe(user => {
      this.userDis = user;
    });
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
    this.infoUserService.getAllInfo(this.userId).subscribe({
      next: (profiles) => {
        this.profiles = profiles;
        // Only select the active profile, do not default to the first one
        this.selectedProfile = profiles.find(p => p.isActive) || undefined;
      },
      error: () => {
        this.errorMessage = 'Could not load profiles.';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  onProfileSelect(profile: InfoUserDTO) {
    this.selectedProfile = profile;
  }

  setActiveProfile() {
    if (!this.selectedProfile) return;
    this.infoUserService.setActive(this.userId, this.selectedProfile).subscribe({
      next: () => {
        this.successMessage = 'Profile set as active!';
        this.loadProfiles();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: () => {
        this.errorMessage = 'Could not set profile as active.';
        setTimeout(() => this.errorMessage = '', 3000);
      }
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
    // If editing an existing profile
    if (this.selectedProfile && this.selectedProfile !== this.editingProfile) {
      this.infoUserService.updateInfo(this.userId, this.selectedProfile, this.editingProfile).subscribe({
        next: () => {
          this.successMessage = 'Profile updated successfully!';
          this.editingProfile = undefined;
          this.loadProfiles();
          setTimeout(() => this.successMessage = '', 3000);
        },
        error: () => {
          this.errorMessage = 'Could not update profile.';
          setTimeout(() => this.errorMessage = '', 3000);
        }
      });
    } else {
      // Add new profile
      this.infoUserService.addInfo(this.userId, this.editingProfile).subscribe({
        next: () => {
          this.successMessage = 'Profile added successfully!';
          const addedProfile = { ...this.editingProfile }; // Save for later selection
          this.editingProfile = undefined;
          this.loadProfiles();
          // Optionally, select the newly added profile after reload
          setTimeout(() => {
            const newProfile = this.profiles.find(p =>
              p.declarant === addedProfile.declarant &&
              p.universitate === addedProfile.universitate
            );
            this.selectedProfile = newProfile;
          }, 500);
          setTimeout(() => this.successMessage = '', 3000);
        },
        error: () => {
          this.errorMessage = 'Could not add profile.';
          setTimeout(() => this.errorMessage = '', 3000);
        }
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
    // Implement as needed
  }

  ngOnDestroy() {
    this.logoutSubscription?.unsubscribe();
  }
}