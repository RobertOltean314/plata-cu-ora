import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

interface FeatureTile {
  title: string;
  description: string;
  icon: string;
  route: string;
  available: boolean;   
}

@Component({
  selector: 'app-homepage',
  templateUrl: './homepage.component.html',
  styleUrls: ['./homepage.component.css'],
  standalone: true,
  imports: [CommonModule]
})
export class HomepageComponent {
  username: string = '';
  
  features: FeatureTile[] = [
    {
      title: 'Calendar Academic',
      description: 'Vizualizați calendarul cu zilele lucrătoare și săptămânile academice',
      icon: 'bi bi-calendar-week',
      route: '/calendar',
      available: true
    },
    {
      title: 'Generator Declarații',
      description: 'Generați declarații pentru perioada selectată (în curând)',
      icon: 'bi bi-file-earmark-pdf',
      route: '/pdf-generator',
      available: false
    },
    {
      title: 'Profil',
      description: 'Gestionați datele profilului dvs.',
      icon: 'bi bi-person-circle',
      route: '/user-profile',
      available: true
    }
  ];

  constructor(private router: Router) {
    // TODO: Fetch the username from a service or state management
    this.username = 'Utilizator'; // Placeholder
  }

  navigateToFeature(route: string, available: boolean): void {
    if (available) {
      this.router.navigate([route]);
    }
  }
}