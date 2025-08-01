import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, ActivatedRoute } from '@angular/router';
import { EditBlogComponent } from './edit-blog.component';
import { BlogService } from '../../../../core/services/blog.service';
import { BlogStateService } from '../../../../core/services/blog-state.service';
import { ImageUploadService } from '../../../../core/services/image-upload.service';
import { of } from 'rxjs';

describe('EditBlogComponent', () => {
  let component: EditBlogComponent;
  let fixture: ComponentFixture<EditBlogComponent>;
  let mockBlogService: jasmine.SpyObj<BlogService>;
  let mockBlogStateService: jasmine.SpyObj<BlogStateService>;
  let mockImageUploadService: jasmine.SpyObj<ImageUploadService>;
  let mockRouter: jasmine.SpyObj<Router>;

const mockBlog = {
    _id: '123',
    user_id: '1',
    username: 'john_doe',
    title: 'Test Blog',
    content: 'Test content',
    tags: ['test'],
    main_image_url: '',
    published: true,
    created_at: new Date().toISOString(),
    updated_at: new Date().toISOString()
  };

  beforeEach(async () => {
    const blogServiceSpy = jasmine.createSpyObj('BlogService', ['getBlogById', 'updateBlog', 'getTags']);
    const blogStateServiceSpy = jasmine.createSpyObj('BlogStateService', ['getBlogById', 'clearSelectedBlog', 'resetChanges', 'updateSelectedBlogContent'], {
      selectedBlog$: of(mockBlog),
      hasChanges$: of(false)
    });
    const imageUploadServiceSpy = jasmine.createSpyObj('ImageUploadService', ['uploadImage']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [EditBlogComponent],
      providers: [
        { provide: BlogService, useValue: blogServiceSpy },
        { provide: BlogStateService, useValue: blogStateServiceSpy },
        { provide: ImageUploadService, useValue: imageUploadServiceSpy },
        { provide: Router, useValue: routerSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { 
              paramMap: {
                get: () => '123'
              }
            }
          }
        }
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EditBlogComponent);
    component = fixture.componentInstance;
    mockBlogService = TestBed.inject(BlogService) as jasmine.SpyObj<BlogService>;
    mockBlogStateService = TestBed.inject(BlogStateService) as jasmine.SpyObj<BlogStateService>;
    mockImageUploadService = TestBed.inject(ImageUploadService) as jasmine.SpyObj<ImageUploadService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    mockBlogService.getTags.and.returnValue(of([]));
    mockBlogStateService.getBlogById.and.returnValue(of(mockBlog));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load blog on init', () => {
    expect(mockBlogStateService.getBlogById).toHaveBeenCalledWith('123');
    expect(component.blogTitle).toBe('Test Blog');
  });

  it('should update blog on form submission', () => {
mockBlogService.updateBlog.and.returnValue(of(mockBlog));

    component.blogTitle = 'Updated Blog';
    component.blogBlocks = [{ id: '1', type: 'content', data: 'Updated content' }];
    component.selectedTags = ['updated'];

    component.performSave();

    expect(mockBlogService.updateBlog).toHaveBeenCalledWith('123', jasmine.objectContaining({
      title: 'Updated Blog',
      content: jasmine.stringMatching(/Updated content/),
      tags: ['updated']
    }));
  });
});
