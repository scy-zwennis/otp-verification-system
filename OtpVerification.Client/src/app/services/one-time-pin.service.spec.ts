import { TestBed } from '@angular/core/testing';

import { OneTimePinService } from './one-time-pin.service';

describe('OneTimePinService', () => {
  let service: OneTimePinService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(OneTimePinService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
