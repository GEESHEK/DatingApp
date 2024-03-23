export interface Pagination {
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

//T is the list of things we return
export class PaginatedResult<T> {
    result?: T
    pagination?: Pagination; //get this from the pagination header 
}