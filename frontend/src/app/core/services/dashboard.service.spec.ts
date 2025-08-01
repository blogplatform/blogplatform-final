import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { DashboardService } from './dashboard.service';
import { Total } from '../../shared/interfaces/dashboard.interface';
import { environment } from '../../../environments/environment';

describe('DashboardService', () => {
  let service: DashboardService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [DashboardService]
    });
    service = TestBed.inject(DashboardService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get totals', () => {
    const mockTotals: Total = {
      total_posts: 10,
      total_users: 5,
      total_likes: 50,
      total_comments: 25
    };

    service.getTotals().subscribe(totals => {
      expect(totals).toEqual(mockTotals);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/dashboard/totals`);
    expect(req.request.method).toBe('GET');
    req.flush(mockTotals);
  });
});
