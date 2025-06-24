import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FeatureTile } from '../../models/featureTile.model';
import { UserService } from '../services/user-services/user.service';
import { Subscription, take } from 'rxjs';
import { User } from '../../models/user.model';


@Component({
  selector: 'app-homepage',
  templateUrl: './homepage.component.html',
  styleUrls: ['./homepage.component.css'],
  standalone: true,
  imports: [CommonModule]
})
export class HomepageComponent implements OnInit, OnDestroy {
  user: any;
  getLoggedInUserSubscription?: Subscription;
  
  features: FeatureTile[] = [
    {
      title: 'Calendar Academic',
      description: 'Vizualizați calendarul cu zilele lucrătoare și săptămânile academice',
      icon: 'bi bi-calendar-week',
      route: '/calendar',
      available: true
    },
    // {
    //   title: 'Generator Declarații',
    //   description: 'Generați declarații pentru perioada selectată (în curând)',
    //   icon: 'bi bi-file-earmark-pdf',
    //   route: '/declaration-generator',
    //   available: true
    // },
    {
      title: 'Profil',
      description: 'Gestionați datele profilului dvs.',
      icon: 'bi bi-person-circle',
      route: '/user-profile',
      available: true
    },
    {
      title: 'Orar',
      description: 'Vizualizați și editați lista orelor ',
      icon: 'bi bi-clock-history', // poți schimba cu orice icon Bootstrap vrei
      route: '/orar',
      available: true
    },
    {
      title: 'Structura an universitar',
      description: 'Vizualizați și editați structura anului universitar ',
      icon: 'bi bi-clipboard', 
      route: '/structura-an',
      available: true
    }
  ];

  constructor(private router: Router, private userService: UserService) {
  }
  ngOnInit(): void {
    this.getLoggedInUserSubscription = this.userService.getLoggedInUser().subscribe(user => {
      this.user = user;
    });
  }
  
  getDisplayName(): string {
    return this.user?.displayName || 'Utilizator';
  }

  ngOnDestroy(): void {
    this.getLoggedInUserSubscription?.unsubscribe();
  }

  navigateToFeature(route: string, available: boolean): void {
    if (available) {
      this.router.navigate([route]);
    }
  }
}