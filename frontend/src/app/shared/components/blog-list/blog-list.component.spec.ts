import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { BlogListComponent } from './blog-list.component';

describe('BlogListComponent', () => {
  let component: BlogListComponent;
  let fixture: ComponentFixture<BlogListComponent>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockPosts = [
    {
      _id: '1',
      title: 'Blog 1',
      content: 'Content 1',
      tags: ['tag1'],
      username: 'johndoe',
      published: true,
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString(),
      user_id: '1'
    },
    {
      _id: '2',
      title: 'Blog 2',
      content: 'Content 2',
      tags: ['tag2'],
      username: 'janesmith',
      published: true,
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString(),
      user_id: '2'
    }
  ];

  beforeEach(async () => {
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [BlogListComponent],
      providers: [
        { provide: Router, useValue: routerSpy }
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(BlogListComponent);
    component = fixture.componentInstance;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    component.posts = mockPosts;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display posts', () => {
    expect(component.posts).toEqual(mockPosts);
    expect(component.posts.length).toBe(2);
  });

  it('should emit tagClick event when tag is clicked', () => {
    spyOn(component.tagClick, 'emit');
    
    component.onTagClick('Angular');
    
    expect(component.tagClick.emit).toHaveBeenCalledWith('Angular');
  });

  it('should navigate to blog detail', () => {
    component.navigateToBlogDetail('123');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/posts/detail', '123']);
  });
});
