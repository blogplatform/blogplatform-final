import { normalizeTag, normalizeTags, areTagsEqual } from './tag.util';

describe('TagUtil', () => {
  it('should normalize single tag', () => {
    const result = normalizeTag('  Angular  ');
    expect(result).toBe('angular');
  });

  it('should normalize tags array', () => {
    const tags = ['  Angular  ', 'JavaScript', '  TypeScript  '];
    const result = normalizeTags(tags);

    expect(result.length).toBe(3);
    expect(result).toEqual(['angular', 'javascript', 'typescript']);
  });

  it('should filter out empty tags', () => {
    const tags = ['Angular', '', '   ', 'React'];
    const result = normalizeTags(tags);
    expect(result).toEqual(['angular', 'react']);
  });

  it('should compare tags correctly', () => {
    expect(areTagsEqual('Angular', 'angular')).toBe(true);
    expect(areTagsEqual('  Angular  ', 'ANGULAR')).toBe(true);
    expect(areTagsEqual('Angular', 'React')).toBe(false);
  });

  it('should handle empty normalization', () => {
    const result = normalizeTags([]);
    expect(result.length).toBe(0);
  });
});

