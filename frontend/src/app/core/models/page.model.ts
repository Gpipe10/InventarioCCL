export interface Page<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  total: number;
}