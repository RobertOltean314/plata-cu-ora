import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StructuraSemestruComponent } from './structura-semestru.component';

describe('StructuraSemestruComponent', () => {
  let component: StructuraSemestruComponent;
  let fixture: ComponentFixture<StructuraSemestruComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StructuraSemestruComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StructuraSemestruComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
