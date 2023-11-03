import { HttpClient, HttpParams } from "@angular/common/http";
import { PaginatedResult } from "../_models/pagination";
import { map } from "rxjs";

export function getPaginatedResult<T>(http: HttpClient, url: string, params: HttpParams) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();

    // the http.get<>() method needs to be modified to get access to the full http response in order to pass the params.
    return http.get<T>(url, { observe: 'response', params }).pipe(
        map(response => {
            if (response.body) {
                paginatedResult.result = response.body;
            }
            const pagination = response.headers.get('Pagination');
            if (pagination) {
                paginatedResult.pagination = JSON.parse(pagination);
            }
            return paginatedResult;
        })
    );
}

export function getPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    params = params.append('pageNumber', pageNumber);
    params = params.append('pageSize', pageSize);

    return params;
}