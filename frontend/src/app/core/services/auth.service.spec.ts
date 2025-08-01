import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should handle login', () => {
    const mockCredentials = { email: 'test@example.com', password: 'password123' };
    const mockResponse = {
      access_token: 'jwt-token',
      refresh_token: 'refresh-token',
      token_type: 'Bearer',
      user: { id: '1', email: 'test@example.com', username: 'testuser' }
    };

    service.login(mockCredentials).subscribe(response => {
      expect(response.access_token).toBe('jwt-token');
      expect(response.user.email).toBe('test@example.com');
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockCredentials);
    req.flush(mockResponse);
    
    // Handle the logout request that gets triggered during token expiry handling
    const logoutReq = httpMock.expectOne(`${environment.apiUrl}/auth/logout`);
    logoutReq.flush({});
  });

  it('should handle registration', () => {
    const mockUserData = {
      username: 'testuser',
      email: 'test@example.com',
      password: 'password123'
    };
    const mockResponse = {
      _id: '1',
      username: 'testuser',
      email: 'test@example.com',
      created_at: new Date().toISOString()
    };

    service.register(mockUserData).subscribe(response => {
      expect(response.username).toBe('testuser');
      expect(response.email).toBe('test@example.com');
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/register`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should handle logout', () => {
    service.logout();
    
    // Expect the logout HTTP request
    const req = httpMock.expectOne(`${environment.apiUrl}/auth/logout`);
    expect(req.request.method).toBe('POST');
    req.flush({});
    
    expect(service.isAuthenticated()).toBe(false);
  });
});
