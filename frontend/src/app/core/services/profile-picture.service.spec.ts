import { TestBed } from '@angular/core/testing';
import { ProfilePictureService } from './profile-picture.service';

describe('ProfilePictureService', () => {
  let service: ProfilePictureService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ProfilePictureService]
    });
    service = TestBed.inject(ProfilePictureService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get profile picture URL', () => {
    const profilePicture = 'uploads/profile-123.jpg';
    const result = service.getProfilePictureUrl(profilePicture);
    expect(result).toBe('https://blog-app-2025.s3.amazonaws.com/uploads/profile-123.jpg');
  });

  it('should return null for empty profile picture', () => {
    const result = service.getProfilePictureUrl('');
    expect(result).toBeNull();
  });

  it('should get user initials', () => {
    const user = { username: 'testuser' };
    const result = service.getUserInitials(user);
    expect(result).toBe('T');
  });

  it('should return default initial for no user', () => {
    const result = service.getUserInitials(null);
    expect(result).toBe('U');
  });
});

