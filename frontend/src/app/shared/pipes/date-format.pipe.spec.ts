import { DateFormatPipe } from './date-format.pipe';

describe('DateFormatPipe', () => {
  let pipe: DateFormatPipe;

  beforeEach(() => {
    pipe = new DateFormatPipe();
  });

  it('should create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  it('should format date string', () => {
    const dateString = '2024-01-01T00:00:00.000Z';
    const result = pipe.transform(dateString);
    expect(result).toBeDefined();
    expect(typeof result).toBe('string');
  });

  it('should format Date object', () => {
    const date = new Date('2024-01-01');
    const result = pipe.transform(date);
    expect(result).toBeDefined();
    expect(typeof result).toBe('string');
  });

  it('should handle null input', () => {
    const result = pipe.transform(null as any);
    expect(result).toBe('');
  });

  it('should handle undefined input', () => {
    const result = pipe.transform(undefined as any);
    expect(result).toBe('');
  });

  it('should use relative format when provided', () => {
    const date = new Date('2024-01-01');
    const result = pipe.transform(date, 'relative');
    expect(result).toBeDefined();
    expect(typeof result).toBe('string');
  });
});
