import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { LikeService } from './like.service';
import { environment } from '../../../environments/environment';

describe('LikeService', () => {
  let service: LikeService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [LikeService]
    });
    service = TestBed.inject(LikeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should toggle like', () => {
    const blogId = '123';
    const mockResponse = {
      _id: '1',
      user_id: 'user1',
      blog_id: blogId,
      created_at: new Date().toISOString()
    };

    service.toggleLike(blogId).subscribe(response => {
      expect(response).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/likes/blogs/${blogId}`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should remove like', () => {
    const blogId = '123';
    const mockResponse = { message: 'Like removed successfully' };

    service.removeLike(blogId).subscribe(response => {
      expect(response).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/likes/blogs/${blogId}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(mockResponse);
  });

  it('should get blog likes count', () => {
    const blogId = '123';
    const mockCount = 10;

    service.getBlogLikesCount(blogId).subscribe(count => {
      expect(count).toEqual(mockCount);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/likes/blogs/${blogId}/count`);
    expect(req.request.method).toBe('GET');
    req.flush(mockCount);
  });
});
