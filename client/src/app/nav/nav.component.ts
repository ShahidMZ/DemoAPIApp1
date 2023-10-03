import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { BsDropdownConfig } from 'ngx-bootstrap/dropdown';
import { Observable, of } from 'rxjs';
import { IUser } from '../_models/user';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css'],
  providers: [{ provide: BsDropdownConfig, useValue: { isAnimated: true, autoClose: true } }]
})
export class NavComponent implements OnInit {
  model: any = {}
  // loggedIn = false;
  // Use rxjs.of() to assign an initial null value to an observable
  // currentUser$: Observable<IUser | null> = of(null);

  constructor(public accountService: AccountService) {}
  
  ngOnInit(): void {
    // this.getCurrentUser();
    // this.currentUser$ = this.accountService.currentUser$;
  }

  // getCurrentUser() {
  //   this.accountService.currentUser$.subscribe({
  //     // !!user returns a boolean. Returns true or false if user is present or absent.
  //     next: user => this.loggedIn = !!user,
  //     error: error => console.log(error),
  //   })
  // }

  login() {
    // Since login() returns an observable, we need to use the subscribe function.
    this.accountService.login(this.model).subscribe({
      next: response => {
        console.log(response);
        // this.loggedIn = true;
      },
      error: error => console.log(error)
    })
  }

  logout() {
    this.accountService.logout();
    // this.loggedIn = false;
  }

}
