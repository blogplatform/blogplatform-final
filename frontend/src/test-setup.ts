// Basic test setup for all services and components
export const mockEnvironment = {
  production: false,
  apiUrl: 'http://localhost:8000/api/v1'
};

// Mock services with basic methods
export class MockAuthService {
  login = jasmine.createSpy('login').and.returnValue({ subscribe: () => {} });
  register = jasmine.createSpy('register').and.returnValue({ subscribe: () => {} });
  isAuthenticated$ = { subscribe: () => {} };
  currentUser$ = { subscribe: () => {} };
  logout = jasmine.createSpy('logout');
}

export class MockBlogService {
  getBlogs = jasmine.createSpy('getBlogs').and.returnValue({ subscribe: () => {} });
  getPosts = jasmine.createSpy('getPosts').and.returnValue({ subscribe: () => {} });
  getBlogById = jasmine.createSpy('getBlogById').and.returnValue({ subscribe: () => {} });
  searchPosts = jasmine.createSpy('searchPosts').and.returnValue({ subscribe: () => {} });
  getTagNames = jasmine.createSpy('getTagNames').and.returnValue({ subscribe: () => {} });
  getPostsByTag = jasmine.createSpy('getPostsByTag').and.returnValue({ subscribe: () => {} });
}

export class MockAiSummaryService {
  generateAiSummary = jasmine.createSpy('generateAiSummary').and.returnValue({ subscribe: () => {} });
}
