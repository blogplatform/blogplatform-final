import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserAvatarComponent } from './user-avatar.component';
import { ProfilePictureService } from '../../../core/services/profile-picture.service';

class MockProfilePictureService {
  getUserProfilePictureUrl = jasmine.createSpy('getUserProfilePictureUrl').and.returnValue(null);
  getUserInitials = jasmine.createSpy('getUserInitials').and.returnValue('U');
  onImageError = jasmine.createSpy('onImageError');
}

describe('UserAvatarComponent', () => {
  let component: UserAvatarComponent;
  let fixture: ComponentFixture<UserAvatarComponent>;
  let mockProfilePictureService: MockProfilePictureService;

  beforeEach(async () => {
    mockProfilePictureService = new MockProfilePictureService();

    await TestBed.configureTestingModule({
      imports: [UserAvatarComponent],
      providers: [
        { provide: ProfilePictureService, useValue: mockProfilePictureService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserAvatarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should get profile picture URL', () => {
    mockProfilePictureService.getUserProfilePictureUrl.and.returnValue('http://example.com/avatar.jpg');
    
    const result = component.getProfilePictureUrl();
    
    expect(result).toBe('http://example.com/avatar.jpg');
    expect(mockProfilePictureService.getUserProfilePictureUrl).toHaveBeenCalledWith(component.user);
  });

  it('should get user initials', () => {
    mockProfilePictureService.getUserInitials.and.returnValue('JD');
    
    const result = component.getUserInitials();
    
    expect(result).toBe('JD');
    expect(mockProfilePictureService.getUserInitials).toHaveBeenCalledWith(component.user);
  });

  it('should handle image error', () => {
    const mockEvent = new Event('error');
    
    component.onImageError(mockEvent);
    
    expect(mockProfilePictureService.onImageError).toHaveBeenCalledWith(mockEvent);
  });

  it('should calculate font size based on avatar size', () => {
    component.size = 40;
    
    const fontSize = component.getFontSize();
    
    expect(fontSize).toBe('14px'); // 40 * 0.35 = 14
  });
});
