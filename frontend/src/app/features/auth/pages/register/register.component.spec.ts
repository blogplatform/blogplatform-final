import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { RegisterComponent } from './register.component';
import { AuthService } from '../../../../core/services/auth.service';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['register']);
    authServiceSpy.isLoading$ = of(false);

    await TestBed.configureTestingModule({
      imports: [RegisterComponent, ReactiveFormsModule, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize register form', () => {
    expect(component.registerForm).toBeDefined();
    expect(component.registerForm.get('email')).toBeDefined();
    expect(component.registerForm.get('password')).toBeDefined();
    expect(component.registerForm.get('confirm_password')).toBeDefined();
  });

  it('should submit register form', () => {
    const mockUser = { 
      _id: '1',
      username: 'testuser', 
      email: 'test@example.com',
      created_at: '2024-01-01T00:00:00Z'
    };
    mockAuthService.register.and.returnValue(of(mockUser));

    // Use a strong password that meets all requirements: uppercase, lowercase, number, special char
    const strongPassword = 'Password123!';
    component.registerForm.patchValue({ 
      email: 'test@example.com',
      password: strongPassword,
      confirm_password: strongPassword
    });
    component.onSubmit();

    expect(mockAuthService.register).toHaveBeenCalledWith({
      username: 'test',
      email: 'test@example.com',
      password: strongPassword,
      confirm_password: strongPassword
    });
  });
  it('should not submit if form is invalid', () => {
    component.registerForm.patchValue({ email: '', password: '', confirm_password: '' });
    component.onSubmit();
    
    expect(mockAuthService.register).not.toHaveBeenCalled();
  });
});
