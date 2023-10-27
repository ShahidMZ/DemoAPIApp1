import { Component, OnInit } from '@angular/core';
import { Observable, take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { IUser } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members: Member[] = [];
  // members$: Observable<Member[]> | undefined;
  // currentUser: string = "";
  pagination: Pagination | undefined;
  userParams: UserParams | undefined;
  // user: IUser | undefined;
  genderList = [{value: 'male', display: 'Male'}, {value: 'female', display: 'Female'}, {value: 'all', display: 'All'}];
  // pageNumber = 1;
  // pageSize = 12;

  constructor(private membersService: MembersService) {
    this.userParams = this.membersService.getUserParams();
    
    // Implemented in MemberService.
    // this.accountService.currentUser$.pipe(take(1)).subscribe({
    //   next: user => {
    //     if (user) {
    //       // this.userParams = new UserParams(user);
    //       this.userParams = membersService.userParams;
    //       this.user = user;
    //     }
    //   }
    // });
  }

  ngOnInit(): void {
    this.loadMembers();
    // this.members$ = this.membersService.getMembers();
    // this.getCurrentUser();
  }

  loadMembers() {
    if (this.userParams) {
      this.membersService.setUserParams(this.userParams);
      this.membersService.getMembers(this.userParams).subscribe({
        next: response => {
          if (response.result && response.pagination) {
            this.members = response.result;
            this.pagination = response.pagination;
          }
        }
      });
    }
  }

  resetFilters() {
    this.userParams = this.membersService.resetUserParams();
    this.loadMembers();
  }

  // My function. Gets the current user's username.
  // getCurrentUser() {
  //   this.accountService.currentUser$.pipe(take(1)).subscribe({
  //     next: user => {
  //       if (!user) return;
  //       this.currentUser = user?.username;
  //     }
  //   })
  // }

  pageChanged(event: any) {
    if (this.userParams?.pageNumber != event.page && this.userParams) {
      this.userParams.pageNumber = event.page;
      this.membersService.setUserParams(this.userParams);
      this.loadMembers();
    }
  }
}
