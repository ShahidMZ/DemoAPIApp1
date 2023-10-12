import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { BsDropdownConfig } from 'ngx-bootstrap/dropdown';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { IUser } from '../_models/user';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css'],
  providers: [{ provide: BsDropdownConfig, useValue: { isAnimated: true, autoClose: true } }]
})
export class NavComponent implements OnInit {
  model: any = {};
  user: IUser | undefined;
  // loggedIn = false;
  // Use rxjs.of() to assign an initial null value to an observable
  // currentUser$: Observable<IUser | null> = of(null);

  constructor(
    public accountService: AccountService, private router: Router, private toastr: ToastrService) {}
  
  ngOnInit(): void {
    this.getCurrentUser();
    // this.currentUser$ = this.accountService.currentUser$;
  }

  getCurrentUser() {
    this.accountService.currentUser$.subscribe({
      // !!user returns a boolean. Returns true or false if user is present or absent.
      next: user => {
        if (!user) return;
        this.user = user
      }
    })
  }

  login() {
    // Since login() returns an observable, we need to use the subscribe function.
    this.accountService.login(this.model).subscribe({
      next: () => this.router.navigateByUrl('/members'),
      error: error => {
        // Handled by the interceptor now.
        // console.log(error);
        // this.toastr.error(error.error);
      }
    })
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
    // this.loggedIn = false;
  }

}
