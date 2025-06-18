import { TestBed } from '@angular/core/testing';


import { OrarService } from './orar-service.service';

describe('OrarService', () => {
  let service: OrarService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(OrarService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
