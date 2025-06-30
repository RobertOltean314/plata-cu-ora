import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule} from '@angular/forms';

import { OrarService } from '../services/orar-services/orar-service.service';
import { OrarEntry } from '../../models/orar-entry.model';
import { UserService } from '../services/user-services/user.service';

@Component({
  selector: 'app-orar',
  templateUrl: './orar.component.html',
  styleUrls: ['./orar.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class OrarComponent implements OnInit {
  orar: OrarEntry[] = [];
  newEntry: OrarEntry = this.getEmptyEntry();
  isEditable = false;
  selectedRow: number | null = null;

  orarUserList: OrarEntry[] = []; 
  editIndex: number | null = null;

  userId: string | null = null; // ID-ul utilizatorului, dacă este necesar

  // Array pentru dropdown-uri
  zileSaptamana = ['Luni', 'Marți', 'Miercuri', 'Joi', 'Vineri', 'Sâmbătă', 'Duminică'];
  tipuriSaptamana = ['Pară', 'Impară'];

  constructor(private orarService: OrarService, private userService: UserService) {
    this.userId = this.userService.getUserId();
  }

  ngOnInit() {
    this.getOrar();
  }

  getOrar() {
    this.orarService.getAll(this.userId!!).subscribe((data: OrarEntry[]) => this.orar = data ?? []);
  }

  getEmptyEntry(): OrarEntry {
    return {
      nrPost: 0,
      denPost: '',
      oreCurs: 0,
      oreSem: 0,
      oreLab: 0,
      oreProi: 0,
      tip: '',
      formatia: '',
      ziua: '',
      imparPar: '',
      materia: '',
      saptamanaInceput: '',
      totalOre: 0
    };
  }

  startAddNew() {
    const newEntry: OrarEntry = {
      nrPost: 0,
      denPost: '',
      oreCurs: 0,
      oreSem: 0,
      oreLab: 0,
      oreProi: 0,
      tip: '',
      formatia: '',
      ziua: '',
      imparPar: '',
      materia: '',
      saptamanaInceput: '',
      totalOre: 0
    };

    this.orarUserList = [newEntry];
    this.editIndex = 0;
  }

  addRow() {
    this.orarService.add(this.userId!!, this.newEntry).subscribe({
      next: (): void => {
        this.getOrar();
        this.newEntry = this.getEmptyEntry();
      }
    });
  }

  deleteRow(index: number) {
    const entry = this.orar[index];
    this.orarService.delete(this.userId!!, entry).subscribe({
      next: (): void => this.getOrar(),
      error: (err: any): void => alert('Eroare la ștergere!')
    });
  }

  startEdit(index: number) {
    this.selectedRow = index;
  }

  saveEdit(index: number) {
    const oldEntry = { ...this.orar[index] };
    const newEntry = this.orar[index];
    this.orarService.update(this.userId!!, oldEntry, newEntry).subscribe({
      next: (): void => {
        this.selectedRow = null;
        this.getOrar();
      },
      error: (err: any): void => alert('Eroare la editare!')
    });
  }

  toggleEdit() {
    this.isEditable = !this.isEditable;
  }
}
