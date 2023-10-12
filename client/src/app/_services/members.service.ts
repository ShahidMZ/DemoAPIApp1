import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { map, of, pipe } from 'rxjs';
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];

  constructor(private http: HttpClient) { }

  // Implemented caching of member list
  getMembers() {
    // If members[] has onjects in it, return the members[] array.
    if (this.members.length > 0) return of(this.members);

    // If members[] is not populated, make an API call to get members, store them in the members[] array and return them.
    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      map(membersIn => {
        this.members = membersIn;
        return membersIn;
      })
    );
  }

  // Implemented caching of member
  getMember(username: string) {
    // Code from Udemy:
    // const member = this.members.find(x => x.userName == username);
    // if (member) return of(member);
    // return this.http.get<Member>(this.baseUrl + 'users/' + username);

    // If the members[] array is populated, get the member from there. Otherwise, make an API call to get it.
    if (this.members.length > 0) 
      return of(this.members.find(member => member.userName == username));

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member);

    // Code from Udemy:
    // Using the spread operator (...).
    // return this.http.put(this.baseUrl + 'users', member).pipe(
    //   map(() => {
    //     const index = this.members.indexOf(member);
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
