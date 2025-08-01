import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { BlogDetailComponent } from './blog-detail.component';
import { BlogService } from '../../../../core/services/blog.service';
import { CommentService } from '../../../../core/services/comment.service';
import { LikeService } from '../../../../core/services/like.service';
import { AuthService } from '../../../../core/services/auth.service';
import { AiSummaryService } from '../../../../core/services/ai-summary.service';

describe('BlogDetailComponent', () => {
  let component: BlogDetailComponent;
  let fixture: ComponentFixture<BlogDetailComponent>;
  let mockBlogService: jasmine.SpyObj<BlogService>;
  let mockCommentService: jasmine.SpyObj<CommentService>;
  let mockLikeService: jasmine.SpyObj<LikeService>;

  beforeEach(async () => {
    const blogServiceSpy = jasmine.createSpyObj('BlogService', ['getBlogById']);
    const commentServiceSpy = jasmine.createSpyObj('CommentService', ['getBlogComments', 'createComment']);
    const likeServiceSpy = jasmine.createSpyObj('LikeService', ['getMyLikeForBlog', 'toggleLike', 'removeLike', 'getBlogLikesCount']);
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated', 'getCurrentUser']);
    const aiSummaryServiceSpy = jasmine.createSpyObj('AiSummaryService', ['generateAiSummary']);
    
    // Setup default return values
    authServiceSpy.isAuthenticated.and.returnValue(true);
    authServiceSpy.getCurrentUser.and.returnValue({ _id: 'user1', username: 'testuser', email: 'test@example.com' });
    likeServiceSpy.getBlogLikesCount.and.returnValue(of(5));

    await TestBed.configureTestingModule({
      imports: [BlogDetailComponent, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: BlogService, useValue: blogServiceSpy },
        { provide: CommentService, useValue: commentServiceSpy },
        { provide: LikeService, useValue: likeServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
        { provide: AiSummaryService, useValue: aiSummaryServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            params: of({ id: '123' }),
            snapshot: {
              paramMap: {
                get: (key: string) => key === 'id' ? '123' : null
              }
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BlogDetailComponent);
    component = fixture.componentInstance;
    mockBlogService = TestBed.inject(BlogService) as jasmine.SpyObj<BlogService>;
    mockCommentService = TestBed.inject(CommentService) as jasmine.SpyObj<CommentService>;
    mockLikeService = TestBed.inject(LikeService) as jasmine.SpyObj<LikeService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load blog details on init', () => {
    const mockBlog = {
      _id: '123',
      title: 'Test Blog',
      content: 'Test content',
      username: 'testuser',
      published: true,
      created_at: '2023-01-01',
      updated_at: '2023-01-01',
      user_id: 'user123'
    };
    const mockComments = [{ _id: '1', text: 'Great post!', user_name: 'user1', blog_id: '123', user_id: 'user1', created_at: '2023-01-01', updated_at: '2023-01-01' }];
    const mockLikeStatus = { _id: 'like1', blog_id: '123', user_id: 'user1', created_at: '2023-01-01', message: 'success' };

    mockBlogService.getBlogById.and.returnValue(of(mockBlog));
    mockCommentService.getBlogComments.and.returnValue(of(mockComments));
    mockLikeService.getMyLikeForBlog.and.returnValue(of(mockLikeStatus));

    component.ngOnInit();

    expect(mockBlogService.getBlogById).toHaveBeenCalledWith('123');
    expect(mockCommentService.getBlogComments).toHaveBeenCalledWith('123');
    expect(mockLikeService.getMyLikeForBlog).toHaveBeenCalledWith('123');
  });

  it('should toggle like status', () => {
    const mockBlog = {
      _id: '123', 
      title: 'Test', 
      content: 'Content', 
      username: 'user',
      user_id: 'user123',
      tags: [],
      published: true,
      created_at: '2023-01-01',
      updated_at: '2023-01-01'
    };
    component.blog = mockBlog;
    component.blogId = '123';
    component.isLiked = false;
    component.likesLoading = false;
    
    // Ensure authentication check passes
    const authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    authService.isAuthenticated.and.returnValue(true);
    
    mockLikeService.toggleLike.and.returnValue(of({ _id: 'like2', blog_id: '123', user_id: 'user1', created_at: '2023-01-01', message: 'liked' }));
    
    component.toggleLike();
    
    expect(mockLikeService.toggleLike).toHaveBeenCalledWith('123');
  });
});
