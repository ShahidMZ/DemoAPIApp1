import { Component, OnInit } from '@angular/core';
import { Observable, take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  // members: Member[] = [];
  members$: Observable<Member[]> | undefined;
  currentUser: string = "";

  constructor(private membersService: MembersService, private accountService: AccountService) { }

  ngOnInit(): void {
    // this.loadMembers();
    this.members$ = this.membersService.getMembers();
    this.getCurrentUser();
  }

  // loadMembers() {
  //   this.membersService.getMembers().subscribe({
  //     next: memberList => this.members = memberList
  //   })
  // }

  // My function. Gets the current user's username.
  getCurrentUser() {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (!user) return;
        this.currentUser = user?.username
      }
    })
  }
}
