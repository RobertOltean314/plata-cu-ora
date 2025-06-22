import { TestBed } from '@angular/core/testing';

import { InfoUserService } from './user-info.service';

describe('UserInfoService', () => {
  let service: InfoUserService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(InfoUserService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
