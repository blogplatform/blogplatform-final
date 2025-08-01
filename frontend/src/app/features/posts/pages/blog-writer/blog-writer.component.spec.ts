import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { BlogWriterComponent } from './blog-writer.component';
import { BlogService } from '../../../../core/services/blog.service';
import { AuthService } from '../../../../core/services/auth.service';
import { ImageUploadService } from '../../../../core/services/image-upload.service';
import { of } from 'rxjs';

describe('BlogWriterComponent', () => {
  let component: BlogWriterComponent;
  let fixture: ComponentFixture<BlogWriterComponent>;
  let mockBlogService: jasmine.SpyObj<BlogService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockImageUploadService: jasmine.SpyObj<ImageUploadService>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    const blogServiceSpy = jasmine.createSpyObj('BlogService', ['createBlog', 'getTags']);
    const authServiceSpy = jasmine.createSpyObj('AuthService', [], {
      isAuthenticated$: of(true)
    });
    const imageUploadServiceSpy = jasmine.createSpyObj('ImageUploadService', ['uploadImage']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [BlogWriterComponent],
      providers: [
        { provide: BlogService, useValue: blogServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
        { provide: ImageUploadService, useValue: imageUploadServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(BlogWriterComponent);
    component = fixture.componentInstance;
    mockBlogService = TestBed.inject(BlogService) as jasmine.SpyObj<BlogService>;
    mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    mockImageUploadService = TestBed.inject(ImageUploadService) as jasmine.SpyObj<ImageUploadService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    
    // Mock getTags method
    mockBlogService.getTags.and.returnValue(of([]));
    
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty blog data', () => {
    expect(component.blogTitle).toBe('');
    expect(component.blogBlocks).toEqual([]);
    expect(component.selectedTags).toEqual([]);
  });

  it('should add a new block', () => {
    const initialLength = component.blogBlocks.length;
    component.addBlock('content');
    expect(component.blogBlocks.length).toBe(initialLength + 1);
    expect(component.blogBlocks[0].type).toBe('content');
  });

  it('should remove a block', () => {
    component.addBlock('content');
    const blockId = component.blogBlocks[0].id;
    component.removeBlock(blockId);
    expect(component.blogBlocks.length).toBe(0);
  });

  it('should publish blog', () => {
    const mockBlog = {
      _id: '123',
      user_id: 'user1',
      username: 'testuser',
      title: 'Test Blog',
      content: 'Test content', 
      tags: ['test'],
      main_image_url: '',
      published: true,
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString(),
      likes_count: 0,
      comment_count: 0
    };

    mockBlogService.createBlog.and.returnValue(of(mockBlog));

    component.blogTitle = 'Test Blog';
    component.blogBlocks = [{ id: '1', type: 'content', data: 'Test content' }];
    component.selectedTags = ['test'];

    component.publishBlog();

    expect(mockBlogService.createBlog).toHaveBeenCalled();
  });
});
