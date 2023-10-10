import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { IUser } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  // Access the Angular form from the HTML template using the @ViewChild directive.
  @ViewChild('editForm') editForm: NgForm | undefined;
  // Access browser level events using HostListener().
  // If the editForm is dirty and the user tries to go to a different URL, show them a warning prompt.
  @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any) {
    if (this.editForm?.dirty) {
      $event.returnValue = true;
    }
  }
  member: Member | undefined;
  // Initialize this.user to null because the user returned by the accountService has type (IUser | null).
  // undefined and null are not the same.
  user: IUser | null = null;

  constructor(private accountService: AccountService, private memberService: MembersService, private toastrService: ToastrService) {
    // Get user from Account Service.
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => this.user = user
    })
  }

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    if (!this.user) return;

    // Get member from Member Service.
    this.memberService.getMember(this.user.username).pipe(take(1)).subscribe({
      next: member => this.member = member
    })
  }

  updateMember() {
    // Cannot just pass the this.member object to the updateMember() funciton. Need to get the values from the editForm directly, pass it to the method and subscribe to it.
    this.memberService.updateMember(this.editForm?.value).subscribe({
      next: () => {
        this.toastrService.success('Profile updated successfully');

        // Reset the form and bind it to the member object once update is complete.
        // The "name" property of individual fields is used to bind the form to the member's contents.
        this.editForm?.reset(this.member);
      }
    });
  }
}