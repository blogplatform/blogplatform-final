import { TestBed } from '@angular/core/testing';
import { BlogStateService } from './blog-state.service';
import { BlogService } from './blog.service';
import { of } from 'rxjs';
import { Blog } from '../../shared/interfaces/post.interface';

describe('BlogStateService', () => {
  let service: BlogStateService;
  let mockBlogService: jasmine.SpyObj<BlogService>;

  beforeEach(() => {
    const spy = jasmine.createSpyObj('BlogService', ['getMyBlogs', 'getBlogById', 'updateBlog', 'deleteBlog']);

    TestBed.configureTestingModule({
      providers: [
        BlogStateService,
        { provide: BlogService, useValue: spy }
      ]
    });
    service = TestBed.inject(BlogStateService);
    mockBlogService = TestBed.inject(BlogService) as jasmine.SpyObj<BlogService>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should load my blogs', () => {
    const mockBlogs: Blog[] = [
      { 
        _id: '1', 
        title: 'Test Blog 1', 
        content: 'Content 1',
        user_id: 'user1',
        username: 'testuser',
        tags: [],
        published: true,
        created_at: '2024-01-01',
        updated_at: '2024-01-01'
      }
    ];

    mockBlogService.getMyBlogs.and.returnValue(of(mockBlogs));
    
    service.loadMyBlogs().subscribe(blogs => {
      expect(blogs.length).toBe(1);
      expect(blogs[0].title).toBe('Test Blog 1');
    });

    expect(mockBlogService.getMyBlogs).toHaveBeenCalledWith(1, 100);
  });

  it('should get blog by id', () => {
    const mockBlog: Blog = {
      _id: '2', // Use different ID to avoid cache conflict
      title: 'Test Blog',
      content: 'Test Content',
      user_id: 'user1',
      username: 'testuser',
      tags: [],
      published: true,
      created_at: '2024-01-01',
      updated_at: '2024-01-01'
    };

    // Mock getBlogById to return a BlogSummary-like object
    mockBlogService.getBlogById.and.returnValue(of(mockBlog as any));
    
    service.getBlogById('2').subscribe(blog => {
      expect(blog).toBeTruthy();
      expect(blog!.title).toBe('Test Blog');
    });

    expect(mockBlogService.getBlogById).toHaveBeenCalledWith('2');
  });
});
