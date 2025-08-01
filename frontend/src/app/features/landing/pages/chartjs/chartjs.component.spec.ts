import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ChartjsComponent } from './chartjs.component';
import { DashboardService } from '../../../../core/services/dashboard.service';
import { of } from 'rxjs';

class MockDashboardService {
  getTotals = jasmine.createSpy('getTotals').and.returnValue(of({total_posts: 0, total_users: 0, total_likes: 0, total_comments: 0}));
  getPostsOverTime = jasmine.createSpy('getPostsOverTime').and.returnValue(of({labels: [], counts: [], group_by: 'day'}));
  getUsersOverTime = jasmine.createSpy('getUsersOverTime').and.returnValue(of({labels: [], counts: [], group_by: 'day'}));
  getTopTags = jasmine.createSpy('getTopTags').and.returnValue(of([]));
  getPostsByCategory = jasmine.createSpy('getPostsByCategory').and.returnValue(of([]));
  getMostLiked = jasmine.createSpy('getMostLiked').and.returnValue(of([]));
}

describe('ChartjsComponent', () => {
  let component: ChartjsComponent;
  let fixture: ComponentFixture<ChartjsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChartjsComponent],
      providers: [
        { provide: DashboardService, useClass: MockDashboardService }
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ChartjsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have initial totals', () => {
    expect(component.totals).toEqual(jasmine.objectContaining({
      total_posts: jasmine.any(Number),
      total_users: jasmine.any(Number),
      total_likes: jasmine.any(Number),
      total_comments: jasmine.any(Number)
    }));
  });
});
