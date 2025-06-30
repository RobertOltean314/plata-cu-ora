import { Component, OnInit } from '@angular/core';
import { ParitateSaptamana } from '../../models/paritate-sapt.model';
import { ParitateSaptService } from '../services/partitate-sapt-services/paritate-sapt.service';
import { UserService } from '../services/user-services/user.service';

@Component({
  selector: 'app-structura-semestru',
  standalone: false,
  templateUrl: './structura-semestru.component.html',
  styleUrl: './structura-semestru.component.css'
})
export class StructuraSemestruComponent implements OnInit{
  
  paritati: ParitateSaptamana[] = [];
  userId: string | null = null;
  addingNew = false;
  editIndex: number | null = null;
  editEntry: ParitateSaptamana = this.emptyEntry();
  newEntry: ParitateSaptamana = this.emptyEntry();
  originalParitati: ParitateSaptamana[] = [];

  dataError: boolean = false;
  editMode: boolean = false;
  editStatus: 'validated' | 'editing' = 'validated';

  constructor(private paritateService: ParitateSaptService, private userService: UserService) {}

  ngOnInit(): void {
    this.userId = this.userService.getUserId();
    this.loadParitati();
  }

  toggleEditMode() {
    this.editMode = !this.editMode;
    this.editStatus = this.editMode ? 'editing' : 'validated';

    if (this.editMode) {
      this.originalParitati = JSON.parse(JSON.stringify(this.paritati));
    }
  }

  formatParitate(paritate: string): string {
    if (!paritate) return '';
    return paritate.toLowerCase() === 'par' ? 'Pară' : 'Impară';
  }

  emptyEntry(): ParitateSaptamana {
    return { sapt: '', data: '', paritate: 'Pară' };
  }

  loadParitati() {
    if (this.userId) {
      this.paritateService.getParitate(this.userId).subscribe(data => {
        this.paritati = data;
      });
    } else {
      console.warn('UserId is null, cannot load data');
    }
  }


  startAdd() {
    this.addingNew = true;
    this.newEntry = this.emptyEntry();
  }

  cancelAdd() {
    this.addingNew = false;
  }

  saveNew() {
    if (!this.userId) return; 

    if (this.dataError || !this.newEntry.sapt || !this.newEntry.data) {
      alert('Completați corect toate câmpurile obligatorii.');
      return;
    }

    this.paritateService.addOrUpdate(this.userId, [this.newEntry]).subscribe(() => {
      this.addingNew = false;
      this.loadParitati();
    });
  }

  startEdit(index: number) {
    this.editIndex = index;
    this.editEntry = { ...this.paritati[index] };
  }

  cancelEdit() {
    this.editIndex = null;
  }

  validateEntry(entry: ParitateSaptamana): boolean {
    if (!entry.sapt || !entry.data) return false;
    if (entry.paritate !== 'par' && entry.paritate !== 'impar') return false;
    return true;
  }

  saveEdit(index: number) {
    if (!this.userId) return;

    if (!this.validateEntry(this.editEntry)) {
      alert('Completați toate câmpurile obligatorii corect.');
      return;
    }

    const oldEntry = this.paritati[index];
    const newEntry = this.editEntry;
    this.paritateService.update(this.userId, oldEntry, newEntry).subscribe(() => {
      this.editIndex = null;
      this.loadParitati();
    });
  }


  deleteParitate(entry: ParitateSaptamana) {
    if (!this.userId) return;
    
    this.paritateService.delete(this.userId!, entry).subscribe({
      next: () => {
        this.paritati = this.paritati.filter(p => p !== entry);
        alert('Șters cu succes!');
      },
      error: (err) => {
        console.error('Eroare la ștergere:', err);
        alert('A apărut o eroare la ștergere.');
      }
    });

  }

  addRow() {
    this.paritati.push(this.emptyEntry());
  }

  cancelAllChanges() {
    this.paritati = JSON.parse(JSON.stringify(this.originalParitati));
    this.editMode = false;
    this.editStatus = 'validated';
  }

  saveAllChanges() {
    if (this.addingNew && this.editEntry.sapt && this.editEntry.data && this.editEntry.paritate) {
      this.paritati.push({...this.editEntry});
      this.addingNew = false;
      this.editEntry = { sapt: '', data: '', paritate: 'par' };
    }

    this.paritateService.addOrUpdate(this.userId!, this.paritati).subscribe(() => {
      alert('Modificările au fost salvate cu succes!');
      this.editMode = false;
      this.editStatus = 'validated';
      this.loadParitati(); 
    }, error => {
      alert('Eroare la salvare modificări!');
    });
  }

  onDataChange() {
    this.dataError = !this.isValidMondayDate(this.newEntry.data);
  }

  onSaptChange() {
    if (!this.newEntry.sapt) return;

    const lastChar = this.newEntry.sapt.trim().slice(-1);
    if (!lastChar) return;

    const lastDigit = parseInt(lastChar, 10);
    if (isNaN(lastDigit)) return; 

    this.newEntry.paritate = (lastDigit % 2 === 0) ? 'par' : 'impar';
  }

  isValidMondayDate(dateStr: string): boolean {
    const regex = /^(\d{2})\.(\d{2})\.(\d{4})$/;
    const match = regex.exec(dateStr);
    if (!match) return false;

    const day = parseInt(match[1], 10);
    const month = parseInt(match[2], 10) - 1; 
    const year = parseInt(match[3], 10);

    const date = new Date(year, month, day);

    if (date.getFullYear() !== year || date.getMonth() !== month || date.getDate() !== day) return false;

    if (date.getDay() !== 1) return false;

    return true;
  }

}
