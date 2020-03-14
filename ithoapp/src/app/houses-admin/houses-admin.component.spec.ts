import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { HousesAdminComponent } from './houses-admin.component';

describe('HousesAdminComponent', () => {
  let component: HousesAdminComponent;
  let fixture: ComponentFixture<HousesAdminComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ HousesAdminComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HousesAdminComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
