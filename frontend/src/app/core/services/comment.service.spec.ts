import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CommentService } from './comment.service';
import { environment } from '../../../environments/environment';

describe('CommentService', () => {
  let service: CommentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [CommentService]
    });
    service = TestBed.inject(CommentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get comments for blog', () => {
    const blogId = '123';
    const mockComments = [
      { _id: '1', text: 'Great post!', user_name: 'user1', blog_id: blogId, user_id: 'user1', created_at: '2024-01-01', updated_at: '2024-01-01' },
      { _id: '2', text: 'Thanks for sharing', user_name: 'user2', blog_id: blogId, user_id: 'user2', created_at: '2024-01-01', updated_at: '2024-01-01' }
    ];

    service.getBlogComments(blogId).subscribe((comments) => {
      expect(comments.length).toBe(2);
      expect(comments[0].text).toBe('Great post!');
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/comments/blogs/${blogId}?skip=0&limit=20`);
    expect(req.request.method).toBe('GET');
    req.flush(mockComments);
  });

  it('should create comment for blog', () => {
    const blogId = '123';
    const commentData = { text: 'New comment' };
    const mockResponse = { _id: '3', text: 'New comment', user_name: 'user3', blog_id: blogId, user_id: 'user3', created_at: '2024-01-01', updated_at: '2024-01-01' };

    service.createComment(blogId, commentData).subscribe((response) => {
      expect(response.text).toBe('New comment');
      expect(response._id).toBe('3');
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/comments/blogs/${blogId}`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(commentData);
    req.flush(mockResponse);
  });

  it('should get my comments', () => {
    const mockComments = [
      { _id: '1', text: 'My comment', user_name: 'me', blog_id: 'blog1', user_id: 'me', created_at: '2024-01-01', updated_at: '2024-01-01' }
    ];

    service.getMyComments().subscribe((comments) => {
      expect(comments.length).toBe(1);
      expect(comments[0].text).toBe('My comment');
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/comments/my-comments?skip=0&limit=20`);
    expect(req.request.method).toBe('GET');
    req.flush(mockComments);
  });
});
