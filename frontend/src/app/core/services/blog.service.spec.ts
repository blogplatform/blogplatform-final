import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BlogService } from './blog.service';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

describe('BlogService', () => {
  let service: BlogService;
  let httpMock: HttpTestingController;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated', 'getCurrentUser']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        BlogService,
        { provide: AuthService, useValue: authServiceSpy }
      ]
    });
    service = TestBed.inject(BlogService);
    httpMock = TestBed.inject(HttpTestingController);
    mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch blogs', () => {
    const mockResponse = {
      blogs: [
        { _id: '1', title: 'Test Blog 1', content: 'Content 1', username: 'user1' },
        { _id: '2', title: 'Test Blog 2', content: 'Content 2', username: 'user2' }
      ],
      total: 2,
      page: 1,
      limit: 10,
      total_pages: 1
    };

    service.getBlogs().subscribe(response => {
      expect(response.blogs.length).toBe(2);
      expect(response.blogs[0].title).toBe('Test Blog 1');
      expect(response.total).toBe(2);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/blogs`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should fetch blog by ID', () => {
    const mockBlog = {
      _id: '1',
      title: 'Test Blog',
      content: 'Test content',
      username: 'testuser',
      created_at: '2024-01-01T00:00:00Z'
    };

    service.getBlogById('1').subscribe(blog => {
      expect(blog.title).toBe('Test Blog');
      expect(blog._id).toBe('1');
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/blogs/1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockBlog);
  });

  it('should fetch tag names', () => {
    const mockTags = [
      { name: 'Angular', _id: '1' },
      { name: 'TypeScript', _id: '2' }
    ];

    service.getTagNames().subscribe(tagNames => {
      expect(tagNames).toEqual(['Angular', 'TypeScript']);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/tags`);
    expect(req.request.method).toBe('GET');
    req.flush(mockTags);
  });

  it('should search blogs', () => {
    const mockSearchResponse = {
      blogs: [
        { blog: { _id: '1', title: 'Angular Blog', content: 'Angular content' } }
      ],
      total: 1,
      page: 1,
      page_size: 10,
      total_pages: 1
    };

    service.searchBlogs('angular').subscribe(response => {
      expect(response.blogs.length).toBe(1);
      expect(response.total).toBe(1);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/blogs/search/angular?page=1&page_size=10`);
    expect(req.request.method).toBe('GET');
    req.flush(mockSearchResponse);
  });
});
