import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaginationComponent } from './pagination.component';

describe('PaginationComponent', () => {
  let component: PaginationComponent;
  let fixture: ComponentFixture<PaginationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaginationComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PaginationComponent);
    component = fixture.componentInstance;
    
    // Set input properties
    component.currentPage = 1;
    component.totalPages = 5;
    
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have initial properties', () => {
    expect(component.currentPage).toBe(1);
    expect(component.totalPages).toBe(5);
    expect(component.loading).toBe(false);
    expect(component.posts).toEqual([]);
  });

  it('should emit pageChange event when page is changed', () => {
    spyOn(component.pageChange, 'emit');
    
    component.onPageChange(3);
    
    expect(component.pageChange.emit).toHaveBeenCalledWith(3);
  });

  it('should not emit page change for same page', () => {
    spyOn(component.pageChange, 'emit');
    
    component.onPageChange(1); // current page
    
    expect(component.pageChange.emit).not.toHaveBeenCalled();
  });

  it('should not emit page change for invalid page', () => {
    spyOn(component.pageChange, 'emit');
    
    component.onPageChange(0); // invalid page
    component.onPageChange(6); // beyond total pages
    
    expect(component.pageChange.emit).not.toHaveBeenCalled();
  });
});
