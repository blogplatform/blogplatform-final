import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ImageUploadService } from './image-upload.service';
import { ImageUploadResponse } from '../../shared/interfaces/api.interface';
import { environment } from '../../../environments/environment';

describe('ImageUploadService', () => {
  let service: ImageUploadService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ImageUploadService]
    });
    service = TestBed.inject(ImageUploadService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should upload an image', () => {
    const mockFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
    const backendResponse = {
      success: true,
      imageUrl: 'http://example.com/image.jpg',
      message: 'Upload successful'
    };
    
    const expectedResponse: ImageUploadResponse = {
      imageUrl: 'http://example.com/image.jpg',
      url: 'http://example.com/image.jpg'
    };

    service.uploadImage(mockFile).subscribe(response => {
      expect(response).toEqual(expectedResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/images/upload`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body instanceof FormData).toBeTruthy();
    req.flush(backendResponse);
  });
});
