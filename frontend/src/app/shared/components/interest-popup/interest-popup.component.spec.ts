import { ComponentFixture, TestBed } from '@angular/core/testing';
import { InterestPopupComponent } from './interest-popup.component';
import { InterestsService } from '../../../core/services/interests.service';
import { AuthService } from '../../../core/services/auth.service';
import { of } from 'rxjs';

describe('InterestPopupComponent', () => {
  let component: InterestPopupComponent;
  let fixture: ComponentFixture<InterestPopupComponent>;
  let mockInterestsService: jasmine.SpyObj<InterestsService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  const mockInterests = [
    { _id: '1', name: 'Technology', description: 'Tech topics' },
    { _id: '2', name: 'Sports', description: 'Sports topics' }
  ];

  beforeEach(async () => {
    const interestsServiceSpy = jasmine.createSpyObj('InterestsService', ['getInterestSuggestions', 'getUserInterests']);
    const authServiceSpy = jasmine.createSpyObj('AuthService', [], {
      isAuthenticated$: of(true)
    });

    await TestBed.configureTestingModule({
      imports: [InterestPopupComponent],
      providers: [
        { provide: InterestsService, useValue: interestsServiceSpy },
        { provide: AuthService, useValue: authServiceSpy }
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(InterestPopupComponent);
    component = fixture.componentInstance;
    mockInterestsService = TestBed.inject(InterestsService) as jasmine.SpyObj<InterestsService>;
    mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;

    mockInterestsService.getInterestSuggestions.and.returnValue(of(['Technology', 'Programming']));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should check authentication status on init', () => {
    expect(component.isCheckingInterests).toBeFalse();
  });

  it('should close popup when requested', () => {
    spyOn(component.popupClosed, 'emit');
    component.closeInterestsPopup();
    expect(component.popupClosed.emit).toHaveBeenCalled();
  });

  it('should emit completion when interests setup completed', () => {
    spyOn(component.interestsSetupCompleted, 'emit');
    component.onInterestsSetupCompleted();
    expect(component.interestsSetupCompleted.emit).toHaveBeenCalled();
  });
});
