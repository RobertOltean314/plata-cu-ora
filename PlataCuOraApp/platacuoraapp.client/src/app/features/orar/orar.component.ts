import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { OrarService } from '../services/orar-services/orar-service.service';
import { OrarEntry } from '../../models/orar-entry.model';

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

  // Array pentru dropdown-uri
  zileSaptamana = ['Luni', 'Marți', 'Miercuri', 'Joi', 'Vineri', 'Sâmbătă', 'Duminică'];
  tipuriSaptamana = ['Pară', 'Impară'];

  constructor(private orarService: OrarService) {}

  ngOnInit() {
    this.getOrar();
  }

  getOrar() {
    this.orarService.getAll().subscribe((data: OrarEntry[]) => this.orar = data ?? []);
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

  addRow() {
    this.orarService.add(this.newEntry).subscribe({
      next: (): void => {
        this.getOrar();
        this.newEntry = this.getEmptyEntry();
      },
      error: (err: any): void => alert('Eroare la adăugare!')
    });
  }

  deleteRow(index: number) {
    const entry = this.orar[index];
    this.orarService.delete(entry).subscribe({
      next: (): void => this.getOrar(),
      error: (err: any): void => alert('Eroare la ștergere!')
    });
  }

  startEdit(index: number) {
    this.selectedRow = index;
  }

  saveEdit(index: number) {
    const oldEntry = { ...this.orar[index] }; // salvezi vechiul entry
    const newEntry = this.orar[index];
    this.orarService.update(oldEntry, newEntry).subscribe({
      next: (): void => {
        this.selectedRow = null;
        this.getOrar();
      },
      error: (err: any): void => alert('Eroare la editare!')
    });
  }

  toggleEdit() {
    if (this.isEditable) {
      // salvezi totul (opțional)
    }
    this.isEditable = !this.isEditable;
  }
}
