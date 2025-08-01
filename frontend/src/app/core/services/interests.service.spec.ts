import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { InterestsService } from './interests.service';
import { environment } from '../../../environments/environment';

describe('InterestsService', () => {
  let service: InterestsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [InterestsService]
    });
    service = TestBed.inject(InterestsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get interest suggestions', () => {
    const mockSuggestions = ['Technology', 'Sports', 'Music'];

    service.getInterestSuggestions().subscribe(suggestions => {
      expect(suggestions).toEqual(mockSuggestions);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/interests/suggestions`);
    expect(req.request.method).toBe('GET');
    req.flush(mockSuggestions);
  });

  it('should create user interests', () => {
    const interestData = { interests: ['Technology', 'Sports'] };
    const mockResponse = {
      _id: '1',
      user_id: 'user1',
      interests: ['Technology', 'Sports'],
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString()
    };

    service.createUserInterests(interestData).subscribe(response => {
      expect(response).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/interests`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(interestData);
    req.flush(mockResponse);
  });

  it('should get user interests', () => {
    const mockUserInterests = {
      _id: '1',
      user_id: 'user1',
      interests: ['Technology'],
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString()
    };

    service.getUserInterests().subscribe(interests => {
      expect(interests).toEqual(mockUserInterests);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/interests`);
    expect(req.request.method).toBe('GET');
    req.flush(mockUserInterests);
  });
});
