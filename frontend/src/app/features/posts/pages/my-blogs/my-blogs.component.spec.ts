import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { MyBlogsComponent } from './my-blogs.component';
import { BlogStateService } from '../../../../core/services/blog-state.service';
import { of } from 'rxjs';

describe('MyBlogsComponent', () => {
  let component: MyBlogsComponent;
  let fixture: ComponentFixture<MyBlogsComponent>;
  let mockBlogStateService: jasmine.SpyObj<BlogStateService>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(async () => {
const mockBlogs = [
      {
        _id: '1',
        user_id: 'user1',
        username: 'testuser',
        title: 'Test Blog',
        content: 'Content',
        tags: ['test'],
        main_image_url: '',
        published: true,
        created_at: new Date().toISOString(),
        updated_at: new Date().toISOString(),
        likes_count: 0,
        comment_count: 0
      }
    ];

    const blogStateServiceSpy = jasmine.createSpyObj('BlogStateService', ['loadMyBlogs', 'deleteBlog'], {
      blogs$: of(mockBlogs),
      loading$: of(false),
      error$: of(null)
    });
    
    // Make loadMyBlogs return an observable
    blogStateServiceSpy.loadMyBlogs.and.returnValue(of(mockBlogs));
    blogStateServiceSpy.deleteBlog.and.returnValue(of({}));
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [MyBlogsComponent],
      providers: [
        { provide: BlogStateService, useValue: blogStateServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MyBlogsComponent);
    component = fixture.componentInstance;
    mockBlogStateService = TestBed.inject(BlogStateService) as jasmine.SpyObj<BlogStateService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load blogs on init', () => {
    expect(mockBlogStateService.loadMyBlogs).toHaveBeenCalled();
    expect(component.blogs.length).toBeGreaterThan(0);
  });

  it('should call deleteBlog on service', () => {
    const mockBlog = {
      _id: '1',
      user_id: 'user1',
      username: 'testuser',
      title: 'Test Blog',
      content: 'Content',
      tags: ['test'],
      main_image_url: '',
      published: true,
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString(),
      likes_count: 0,
      comment_count: 0
    };

    component.onDeleteBlog(mockBlog);
    component.confirmDelete();

    expect(mockBlogStateService.deleteBlog).toHaveBeenCalledWith('1');
  });

  it('should navigate to edit blog', () => {
    const mockBlog = {
      _id: '1',
      user_id: 'user1',
      username: 'testuser',
      title: 'Test Blog',
      content: 'Content',
      tags: ['test'],
      main_image_url: '',
      published: true,
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString(),
      likes_count: 0,
      comment_count: 0
    };

    component.onEditBlog(mockBlog);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/posts/edit', '1']);
  });
});
