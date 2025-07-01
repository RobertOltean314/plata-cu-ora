import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule} from '@angular/forms';

import { OrarService } from '../services/orar-services/orar-service.service';
import { OrarEntry } from '../../models/orar-entry.model';
import { UserService } from '../services/user-services/user.service';

export interface Orar {
  nrPost: number;
  denPost: 'Prof.'| 'Conf.' | 'Sef L.' | 'Lect.' | 'Asist.';
  oreCurs: number;
  oreSem: number;
  oreLab: number;
  oreProi: number;
  tip: 'LR' | 'MR' | 'LE' | 'ME';
  formatia: string;
  ziua: 'Luni' | 'Marti' | 'Miercuri' | 'Joi' | 'Vineri';
  imparPar: 'Par' | 'Impar' | ' ';
  materia: string;
  saptamanaInceput: string; 
  totalOre?: number; 
}

@Component({
  selector: 'app-orar',
  templateUrl: './orar.component.html',
  styleUrls: ['./orar.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class OrarComponent implements OnInit {
  orar: OrarEntry[] = [];
  isEditable = false;
  addingNew = false;
  newEntry: OrarEntry = this.initNewEntry();
  userId: string | null = null;


  constructor(
    private orarService: OrarService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.userId = this.userService.getUserId();
    if (this.userId) {
      this.loadOrar();
    }
}


  initNewEntry(): OrarEntry {
    return {
      nrPost: 0,
      denPost: '',
      oreCurs: 0,
      oreSem: 0,
      oreLab: 0,
      oreProi: 0,
      tip: 'LR',
      formatia: '',
      ziua: 'Luni',
      imparPar: 'Par',
      materia: '',
      saptamanaInceput: '',
      totalOre: 0
    };
  }

  cancelAdd() {
    this.addingNew = false;
  }

  loadOrar() {
    if (!this.userId) return;
    this.orarService.getAll(this.userId).subscribe({
      next: (data) => {
        this.orar = data;
      },
      error: (err) => {
        console.error('Eroare la încărcare orar:', err);
      }
    });
  }

  toggleEdit() {
    if (this.isEditable) {
      this.saveOrar();
    }
    this.isEditable = !this.isEditable;
    if (!this.isEditable) {
      this.addingNew = false;
    }
  }

  startAddNew() {
    this.addingNew = true;
    this.newEntry = this.initNewEntry();
  }

  addOrar() {
    if (!this.validateEntry(this.newEntry)) {
      alert('Te rog completează toate câmpurile obligatorii corect!');
      return;
    }
    
    this.orarService.add(this.userId!, this.newEntry).subscribe({
      next: (res) => {
        console.log('Intrare adăugată:', res);
        this.orar.push({ ...this.newEntry });
        this.addingNew = false;
      },
      error: (err) => {
        console.error('Eroare la adăugare:', err);
      }
    });
  }


  updateOrar(index: number) {
    const oldEntry = this.orar[index];
    const newEntry = this.orar[index]; // modificată direct în UI

    this.orarService.update(this.userId!, oldEntry, newEntry).subscribe({
      next: (res) => {
        console.log('Intrare actualizată:', res);
      },
      error: (err) => {
        console.error('Eroare la actualizare:', err);
      }
    });
  }

  deleteOrar(index: number) {
    if (!confirm('Sigur dorești să ștergi această intrare?')) {
      return;
    }
    const entryToDelete = this.orar[index];
    this.orarService.delete(this.userId!, entryToDelete).subscribe({
      next: () => {
        this.orar.splice(index, 1);
        console.log('Intrare ștearsă cu succes');
      },
      error: (err) => {
        console.error('Eroare la ștergere:', err);
      }
    });
  }

  validateEntry(entry: OrarEntry): boolean {
    return (
      entry.nrPost > 0 &&
      entry.denPost.trim().length > 0 &&
      entry.oreCurs >= 0 &&
      entry.oreSem >= 0 &&
      entry.oreLab >= 0 &&
      entry.oreProi >= 0 &&
      ['LR', 'MR', 'LE', 'ME'].includes(entry.tip) &&
      entry.formatia.trim().length > 0 &&
      ['Luni', 'Marti', 'Miercuri', 'Joi', 'Vineri'].includes(entry.ziua) &&
      entry.materia.trim().length > 0
    );
  }


  saveOrar() {
    // Poți itera orarul și actualiza fiecare intrare dacă vrei, sau faci update individual
    console.log('Salvare orar apelată');
    // Dacă vrei, poți salva fiecare intrare prin updateOrar(index)
    this.orar.forEach((entry, index) => this.updateOrar(index));
  }

  saveAllChanges() {
    if (this.orar.length === 0) {
      alert('Nu există date de salvat.');
      return;
    }

    // Pentru fiecare element din orar, apelăm updateOrar
    this.orar.forEach((_, index) => {
      this.updateOrar(index);
    });

    this.isEditable = false;
    this.addingNew = false;

    alert('Modificările au fost salvate cu succes!');
  }

}