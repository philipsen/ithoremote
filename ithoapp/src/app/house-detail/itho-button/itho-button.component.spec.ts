import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IthoButtonComponent } from './itho-button.component';

describe('IthoButtonComponent', () => {
  let component: IthoButtonComponent;
  let fixture: ComponentFixture<IthoButtonComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IthoButtonComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IthoButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
