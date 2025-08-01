import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AiSummaryService } from './ai-summary.service';
import { environment } from '../../../environments/environment';

describe('AiSummaryService', () => {
  let service: AiSummaryService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AiSummaryService]
    });
    service = TestBed.inject(AiSummaryService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should generate AI summary', () => {
    const blogId = '123';
    const blogTitle = 'Test Blog Title';
    const mockContent = 'This is test blog content';
    const mockSummary = { _id: 'summary1', blog_id: blogId, summary: 'AI generated summary', created_at:'2024-01-01' };

    service.generateAiSummary(blogId, blogTitle, mockContent).subscribe(result => {
      expect(result).toEqual(mockSummary);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/summaries/`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      blog_id: blogId,
      blog_title: blogTitle,
      blog_content: mockContent
    });
    req.flush(mockSummary);
  });
});
