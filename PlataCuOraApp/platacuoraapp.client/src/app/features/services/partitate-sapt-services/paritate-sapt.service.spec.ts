import { TestBed } from '@angular/core/testing';

import { ParitateSaptService } from './paritate-sapt.service';

describe('ParitateSaptService', () => {
  let service: ParitateSaptService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ParitateSaptService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
