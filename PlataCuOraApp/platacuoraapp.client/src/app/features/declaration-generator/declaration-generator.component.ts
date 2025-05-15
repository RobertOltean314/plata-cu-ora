import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-declaration-generator',
  templateUrl: './declaration-generator.component.html',
  styleUrls: ['./declaration-generator.component.css'],
  standalone:false
})
export class DeclarationGeneratorComponent implements OnInit {
  declarationForm!: FormGroup;
  documentTypes: string[] = ['Adeverință', 'Declarație', 'Cerere'];
  successMessage: string = '';
  errorMessage: string = '';
  
  constructor(
    private fb: FormBuilder,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.initForm();
  }

  initForm(): void {
    this.declarationForm = this.fb.group({
      declarant: ['', Validators.required],
      tip: ['', Validators.required],
      directorDepartament: [''],
      decan: [''],
      universitate: ['', Validators.required],
      facultate: ['', Validators.required],
      departament: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.declarationForm.valid) {
      // Print the form values to console
      console.log('Form values:', this.declarationForm.value);
      
      // Show success message
      this.successMessage = 'Formularul a fost printat în consolă!';
      this.errorMessage = '';
    } else {
      this.errorMessage = 'Vă rugăm să completați toate câmpurile obligatorii.';
      this.successMessage = '';
      this.markFormGroupTouched(this.declarationForm);
    }
  }

  onCancel(): void {
    this.router.navigate(['/']);
  }

  // Helper method to mark all controls as touched
  markFormGroupTouched(formGroup: FormGroup) {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  // Helper methods for form validation
  isFieldInvalid(fieldName: string): boolean {
    const field = this.declarationForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }
  
  isFieldRequired(fieldName: string): boolean {
    const field = this.declarationForm.get(fieldName);
    return field ? field.hasError('required') && (field.dirty || field.touched) : false;
  }
}