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

  saveEdit(index: number) {
    if (!this.userId) return;
    const oldEntry = this.paritati[index];
    const newEntry = this.editEntry;
    this.paritateService.update(this.userId, oldEntry, newEntry).subscribe(() => {
      this.editIndex = null;
      this.loadParitati();
    });
  }

  deleteParitate(entry: ParitateSaptamana) {
    if (!this.userId) return;
    
    this.paritateService.delete(this.userId, entry).subscribe(() => {
      this.loadParitati();
    });
  }

}
