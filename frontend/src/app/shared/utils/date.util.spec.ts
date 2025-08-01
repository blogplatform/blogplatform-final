import { TestBed } from '@angular/core/testing';
import { DateUtil } from './date.util';

describe('DateUtil', () => {
  it('should format date', () => {
    const date = new Date('2024-01-01');
    const result = DateUtil.formatDate(date);
    expect(result).toBeDefined();
  });

  it('should handle null date', () => {
    const result = DateUtil.formatDate(null as any);
    expect(result).toBe('N/A');
  });

  it('should handle undefined date', () => {
    const result = DateUtil.formatDate(undefined as any);
    expect(result).toBe('N/A');
  });

  it('should check if date is today', () => {
    const today = new Date();
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    
    expect(DateUtil.isToday(today)).toBeTrue();
    expect(DateUtil.isToday(yesterday)).toBeFalse();
  });

  it('should format relative time', () => {
    const date = new Date('2024-01-01');
    const result = DateUtil.formatRelativeTime(date);
    expect(result).toBeDefined();
  });
});
