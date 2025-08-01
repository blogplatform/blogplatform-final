import { ComponentFixture, TestBed } from '@angular/core/testing';
import { InterestsComponent } from './interests.component';
import { InterestsService } from '../../../core/services/interests.service';
import { of } from 'rxjs';

describe('InterestsComponent', () => {
  let component: InterestsComponent;
  let fixture: ComponentFixture<InterestsComponent>;
  let mockInterestsService: jasmine.SpyObj<InterestsService>;

  const mockInterests = [
    { _id: '1', name: 'Technology', description: 'Tech topics' },
    { _id: '2', name: 'Sports', description: 'Sports topics' }
  ];

  beforeEach(async () => {
    const interestsServiceSpy = jasmine.createSpyObj('InterestsService', ['getInterestSuggestions', 'getUserInterests', 'createUserInterests', 'updateUserInterests']);

    await TestBed.configureTestingModule({
      imports: [InterestsComponent],
      providers: [
        { provide: InterestsService, useValue: interestsServiceSpy }
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(InterestsComponent);
    component = fixture.componentInstance;
    mockInterestsService = TestBed.inject(InterestsService) as jasmine.SpyObj<InterestsService>;

    mockInterestsService.getInterestSuggestions.and.returnValue(of(['Technology', 'Programming']));
    mockInterestsService.getUserInterests.and.returnValue(of({
      _id: '1',
      user_id: 'user123',
      interests: ['Technology'],
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString()
    }));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load interests on init', () => {
    expect(mockInterestsService.getInterestSuggestions).toHaveBeenCalled();
    expect(mockInterestsService.getUserInterests).toHaveBeenCalled();
    expect(component.suggestions).toEqual(['Technology', 'Programming']);
  });

  it('should toggle interest selection', () => {
    component.toggleInterest('2');
    expect(component.selectedInterests).toContain('2');

    component.toggleInterest('2');
    expect(component.selectedInterests).not.toContain('2');
  });

  it('should save selected interests for new user', () => {
    const mockUserInterests = {
      _id: '1',
      user_id: 'user123',
      interests: ['Technology', 'Programming'],
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString()
    };
    
    // Setup component for new user (no existing interests)
    component.userInterests = null;
    component.selectedInterests = ['Technology', 'Programming'];
    
    // Mock the createUserInterests method
    mockInterestsService.createUserInterests.and.returnValue(of(mockUserInterests));

    component.saveInterests();

    expect(mockInterestsService.createUserInterests).toHaveBeenCalledWith({ interests: ['Technology', 'Programming'] });
  });
});
