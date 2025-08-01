import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { EmailVerificationComponent } from './email-verification.component';
import { AuthService } from '../../../../core/services/auth.service';

describe('EmailVerificationComponent', () => {
  let component: EmailVerificationComponent;
  let fixture: ComponentFixture<EmailVerificationComponent>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['verifyEmail']);

    await TestBed.configureTestingModule({
      imports: [EmailVerificationComponent, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            queryParams: of({ token: 'test-token' }),
            snapshot: {
              queryParamMap: {
                get: (key: string) => key === 'token' ? 'test-token' : null
              }
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EmailVerificationComponent);
    component = fixture.componentInstance;
    mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should verify email on init', () => {
    // Mock non-existent method for now
    const verifyEmailSpy = jasmine.createSpy('verifyEmail').and.returnValue(of({ success: true }));
    (mockAuthService as any).verifyEmail = verifyEmailSpy;

    component.ngOnInit();

    expect(verifyEmailSpy).toHaveBeenCalledWith('test-token');
  });
});
