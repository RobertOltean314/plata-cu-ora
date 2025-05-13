import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { HomepageComponent } from './homepage.component';

describe('HomepageComponent', () => {
  let component: HomepageComponent;
  let fixture: ComponentFixture<HomepageComponent>;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomepageComponent],
      providers: [
        { provide: Router, useValue: { navigate: jasmine.createSpy('navigate') } }
      ]
    }).compileComponents();
    
    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(HomepageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have a list of features', () => {
    expect(component.features.length).toBeGreaterThan(0);
  });

  it('should navigate to route when feature is available', () => {
    const route = '/calendar';
    component.navigateToFeature(route, true);
    expect(router.navigate).toHaveBeenCalledWith([route]);
  });

  it('should not navigate to route when feature is unavailable', () => {
    const route = '/pdf-generator';
    component.navigateToFeature(route, false);
    expect(router.navigate).not.toHaveBeenCalled();
  });
});