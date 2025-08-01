import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../../../../core/services/auth.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['login', 'isAuthenticated', 'isLoading$']);
    authServiceSpy.isAuthenticated.and.returnValue(false);
    authServiceSpy.isLoading$ = of(false);

    await TestBed.configureTestingModule({
      imports: [LoginComponent, ReactiveFormsModule, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize login form', () => {
    expect(component.loginForm).toBeDefined();
    expect(component.loginForm.get('email')).toBeDefined();
    expect(component.loginForm.get('password')).toBeDefined();
  });

  it('should submit login form', () => {
    const mockLoginResponse = { 
      access_token: 'mock-token', 
      refresh_token: 'mock-refresh',
      token_type: 'Bearer',
      user: {
        _id: '1',
        username: 'testuser',
        email: 'test@example.com',
        created_at: '2024-01-01T00:00:00Z'
      }
    };
    mockAuthService.login.and.returnValue(of(mockLoginResponse));

    component.loginForm.patchValue({ email: 'test@example.com', password: 'password123' });
    component.onSubmit();

    expect(mockAuthService.login).toHaveBeenCalledWith({ email: 'test@example.com', password: 'password123' });
  });

  it('should not submit if form is invalid', () => {
    component.loginForm.patchValue({ email: '', password: '' });
    component.onSubmit();
    
    expect(mockAuthService.login).not.toHaveBeenCalled();
  });
});
