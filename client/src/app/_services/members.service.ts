import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { map, of, take } from 'rxjs';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { IUser } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  // Implement the cache that stores key-value pairs where the key is the stringified query passed to the API, ex: 18-99-1-12-lastActive-male.
  memberCache = new Map();
  user: IUser | undefined;
  userParams: UserParams | undefined;
  // members: Member[] = [];
  // paginatedResult: PaginatedResult<Member[]> = new PaginatedResult<Member[]>;

  // It is OK to inject a service into another service, as long as the second service is not injected into the first service, which creates a circular reference.
  // For instance, the AccountService is injected here. Now do NOT inject MemberService into AccountService.
  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) {
          this.userParams = new UserParams(user);
          this.user = user;
        }
      }
    });
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    if (this.user) {
      console.log("In resetUserParams()");
      this.userParams = new UserParams(this.user);
      return this.userParams;
    }

    return;
  }

  getMembers(userParams: UserParams) {
    // Implementing caching: Chapter 169: Restoring the caching for members.
    const response = this.memberCache.get(Object.values(userParams).join('-'));
    if (response) return of(response);

    // Get Members
    let params = this.getPaginationHeaders(userParams.pageNumber, userParams.pageSize);

    params = params.append('minAge', userParams.minAge);
    params = params.append('maxAge', userParams.maxAge);
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    return this.getPaginatedResult<Member[]>(this.baseUrl + 'users', params).pipe(
      map(response => {
        this.memberCache.set(Object.values(userParams).join('-'), response);
        // console.log(this.memberCache);
        return response;
      })
    );
  }

  private getPaginatedResult<T>(url: string, params: HttpParams) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();

    // the http.get<>() method needs to be modified to get access to the full http response in order to pass the params.
    return this.http.get<T>(url, { observe: 'response', params }).pipe(
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

  getPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    params = params.append('pageNumber', pageNumber);
    params = params.append('pageSize', pageSize);

    return params;
  }

  // Before filtering was added (Chapter 162: Cleaning up the member service):
  // getMembers(page?: number, itemsPerPage?: number) {
  //   // Pass the params in the query using HttpParams, which is an immutable class.
  //   let params = new HttpParams();

  //   if (page && itemsPerPage) {
  //     params = params.append('pageNumber', page);
  //     params = params.append('pageSize', itemsPerPage);
  //   }

  //   // the http.get<>() method needs to be modified to get access to the full http response in order to pass the params.
  //   return this.http.get<Member[]>(this.baseUrl + 'users', {observe: 'response', params}).pipe(
  //     map(response => {
  //       if (response.body) {
  //         this.paginatedResult.result = response.body;
  //       }
  //       const pagination = response.headers.get('Pagination');
  //       if (pagination) {
  //         this.paginatedResult.pagination = JSON.parse(pagination)
  //       }
  //       return this.paginatedResult;
  //     })
  //   );
  // }

  // Before implementation of Pagination:
  // Implemented caching of member list
  // getMembers() {
  //   // If members[] has objects in it, return the members[] array.
  //   if (this.members.length > 0) return of(this.members);

  //   // If members[] is not populated, make an API call to get members, store them in the members[] array and return them.
  //   return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
  //     map(membersIn => {
  //       this.members = membersIn;
  //       return membersIn;
  //     })
  //   );
  // }

  getMember(username: string) {
    // Code from Udemy:
    // const member = this.members.find(x => x.userName == username);
    // if (member) return of(member);
    // return this.http.get<Member>(this.baseUrl + 'users/' + username);

    // Removed because we are now using key-value pairs in memberCache instead of the members array.
    // If the members[] array is populated, get the member from there. Otherwise, make an API call to get it.
    // if (this.members.length > 0) 
    //   return of(this.members.find(member => member.userName == username));

    // Spread the values of the memberCache and use the reduce funciton to flatten it and concatenate all the members into a single array.
    // The callback funciton in reduce() is called for every object in the flattened memberCache array.
    // arr is the new empty array (defined by the initialValue of []) that is concatenated to using the elem variable, which is the element of the flattened array.
    // elem represents the paginatedResult that contains the pagination and result properties.
    const member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((member: Member) => member.userName == username);

    // console.log(member);

    if (member) return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member);

    // // Code from Udemy:
    // // Using the spread operator (...).
    // return this.http.put(this.baseUrl + 'users', member).pipe(
    //   map(() => {
    //     const index = this.members.indexOf(member);
    //     // The spread operator spreads the properties of the specified member in the members[] array and copies the properties of the member object to it.
    //     this.members[index] = {...this.members[index], ...member};
    //   })
    // )
  }

  setMainPhoto(photoId: number) {
    // Since this is a put request, pass an empty object as a variable.
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    // No need to pass variables for a delete request.
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }

  // Implemented in JwtInterceptor
  // getHttpOptions() {
  //   const userString = localStorage.getItem('user');
  //   if (!userString) return;
  //   const user = JSON.parse(userString);
  //   return {
  //     headers: new HttpHeaders({
  //       Authorization: 'Bearer ' + user.token
  //     })
  //   }
  // }
}
