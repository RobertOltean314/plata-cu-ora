import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { CalendarComponent } from './calendar.component';

describe('CalendarComponent', () => {
  let component: CalendarComponent;
  let fixture: ComponentFixture<CalendarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CalendarComponent],
      imports: [ReactiveFormsModule]
    })
      .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CalendarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should generate calendar data', () => {
    component.generateCalendarData();
    expect(component.calendarData.length).toBeGreaterThan(0);
  });

  it('should apply date filter correctly', () => {
    const startDate = new Date('2025-05-01');
    const endDate = new Date('2025-05-10');

    component.filterForm.setValue({
      startDate: startDate,
      endDate: endDate
    });

    component.applyDateFilter();

    component.filteredCalendarData.forEach(day => {
      expect(day.date >= startDate && day.date <= endDate).toBe(true);
    });

    expect(component.filteredCalendarData.length).toBe(10);
  });

  it('should format date correctly', () => {
    const testDate = new Date('2025-05-01');
    const formattedDate = component.formatDate(testDate);
    expect(formattedDate).toMatch(/01\.05\.2025|01\/05\/2025/);
  });

  it('should reset filter to default values', () => {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    component.resetFilter();

    const formStartDate = new Date(component.filterForm.get('startDate')?.value);
    formStartDate.setHours(0, 0, 0, 0);

    expect(formStartDate.getDate()).toBe(today.getDate());
    expect(formStartDate.getMonth()).toBe(today.getMonth());
    expect(formStartDate.getFullYear()).toBe(today.getFullYear());

    const endDate = new Date(component.filterForm.get('endDate')?.value);
    const expectedEndDate = new Date(today);
    expectedEndDate.setMonth(expectedEndDate.getMonth() + 1);

    expect(endDate.getMonth()).toBe(expectedEndDate.getMonth());
  });
});
