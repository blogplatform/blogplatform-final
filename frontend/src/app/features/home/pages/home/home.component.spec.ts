import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { HomeComponent } from './home.component';
import { BlogService } from '../../../../core/services/blog.service';
import { AuthService } from '../../../../core/services/auth.service';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let mockBlogService: jasmine.SpyObj<BlogService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    const blogServiceSpy = jasmine.createSpyObj('BlogService', ['getPosts', 'searchPosts', 'getTagNames']);
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated']);
    
    // Set up default returns
    authServiceSpy.isAuthenticated$ = of(false);
    authServiceSpy.currentUser$ = of(null);
    blogServiceSpy.getTagNames.and.returnValue(of(['tag1', 'tag2']));

    await TestBed.configureTestingModule({
      imports: [HomeComponent, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: BlogService, useValue: blogServiceSpy },
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    mockBlogService = TestBed.inject(BlogService) as jasmine.SpyObj<BlogService>;
    mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load blogs on init', () => {
    const mockResponse = {
      posts: [{ _id: '1', title: 'Test Blog', content: 'Content', published: true, user_id: '1', created_at: '', updated_at: '' }],
      page: 1,
      total_pages: 1,
      total: 1,
      limit: 10
    };
    mockBlogService.getPosts.and.returnValue(of(mockResponse));
    
    component.ngOnInit();
    
    expect(mockBlogService.getPosts).toHaveBeenCalled();
  });

  it('should search blogs', () => {
    const mockSearchResults = {
      posts: [{ _id: '1', title: 'Search Result', content: 'Content', published: true, user_id: '1', created_at: '', updated_at: '' }],
      page: 1,
      total_pages: 1,
      total: 1,
      limit: 10
    };
    mockBlogService.searchPosts.and.returnValue(of(mockSearchResults));
    
    // Call the method as private - need to access via any
    (component as any).performSearch('test');
    
    expect(mockBlogService.searchPosts).toHaveBeenCalledWith('test', 1, 10);
  });
});
