import { TestBed } from '@angular/core/testing';

import { Houses2Service } from './houses2.service';

describe('Houses2Service', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: Houses2Service = TestBed.get(Houses2Service);
    expect(service).toBeTruthy();
  });
});
