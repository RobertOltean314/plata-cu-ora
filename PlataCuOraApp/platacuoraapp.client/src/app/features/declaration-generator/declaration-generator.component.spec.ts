import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeclarationGeneratorComponent } from './declaration-generator.component';

describe('DeclarationGeneratorComponent', () => {
  let component: DeclarationGeneratorComponent;
  let fixture: ComponentFixture<DeclarationGeneratorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DeclarationGeneratorComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeclarationGeneratorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
