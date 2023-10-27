export interface Pagination {
    // These variable names should match the names in the PaginationHeaders.cs file.
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

export class PaginatedResult<T> {
    result?: T;
    pagination?: Pagination;
}